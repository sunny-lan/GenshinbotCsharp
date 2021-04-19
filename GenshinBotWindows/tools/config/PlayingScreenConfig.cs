
using genshinbot.database;
using genshinbot.screens;
using OpenCvSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static genshinbot.yui.Ext;

namespace genshinbot.tools.config
{
    class PlayingScreenConfig : Script
    {
        yui.Tab tab;
        public void Load(GenshinBot b)
        {
            Debug.Assert(tab == null);
            Debug.Assert(b.Ui != null);
            Debug.Assert(b.Db != null);

            var ui = b.Ui;
            var db = b.Db.PlayingScreenDb;
            tab = ui.CreateTab();
            tab.Title = "Playing screen";
            var content = tab.Content;
            content.SuspendLayout();
            content.SetFlex(new yui.Flexbox
            {
                Direction = yui.Orientation.Horizontal,
                Scroll=false,
            });

            var vp = content.CreateViewport();
            content.SetFlex(vp, new yui.Flexbox.Item { Weight = 1 });
            vp.OnTChange = t => vp.T = t;

            var img = vp.CreateImage();

            var sidebar = content.CreateSubContainer();
            content.SetFlex(sidebar, new yui.Flexbox.Item { Weight = 0 });
            sidebar.SetFlex(new yui.Flexbox
            {
                Direction = yui.Orientation.Vertical,
                Scroll = true,
                Wrap = false,
            });
            var screenshotBtn = sidebar.CreateButton();
            screenshotBtn.Text = "Screenshot";
            var clearBtn = sidebar.CreateButton();
            clearBtn.Text = "Clear";
            var saveBtn = sidebar.CreateButton();
            saveBtn.Text = "Save";

            var refineBtn = sidebar.CreateButton();
            refineBtn.Text = "Refine";


            var numSatMax = sidebar.CreateSlider();
            numSatMax.Label = "NS";
            numSatMax.Max = 255;
            numSatMax.Min = 0;


            //only enable button when attached
            screenshotBtn.Enabled = b.W != null;
            b.AttachedWindowChanged += (s, attached) => screenshotBtn.Enabled = attached;

            async Task<PlayingScreen.Db.RD.CharacterTemplate[]> askCharacterTemplates()
            {
                var prevStatus = tab.Status;
                var lines = new List<yui.XYLine>();

                async Task<int> askUser(string selectionMessage, yui.Orientation o, int? min = null, int? max = null)
                {
                    tab.Status = selectionMessage;
                    ui.GiveFocus(tab);
                    var r = await yui.XYLine.Select(vp, o, min, max);
                    lines.Add(r);
                    return r.V;
                }


                int nxb = await askUser("Number X begin", yui.Orientation.Vertical);
                int nxe = await askUser("Number X end", yui.Orientation.Vertical);
                if (nxe < nxb) Util.Swap(ref nxe, ref nxb);

                int hxb = await askUser("Health X begin", yui.Orientation.Vertical);
                int hxe = await askUser("Health X end", yui.Orientation.Vertical);
                if (hxe < hxb) Util.Swap(ref hxb, ref hxe);

                var res = new PlayingScreen.Db.RD.CharacterTemplate[4];
                for (int i = 0; i < 4; i++)
                {
                    int nyb = await askUser(i + " Number Y begin", yui.Orientation.Horizontal, nxb, nxe);
                    int nye = await askUser(i + " Number Y end", yui.Orientation.Horizontal, nxb, nxe);
                    if (nye < nyb) Util.Swap(ref nyb, ref nye);

                    var number = new Rect(nxb, nyb, nxe - nxb, nye - nyb);

                    int hyb = await askUser(i + " Health Y begin", yui.Orientation.Horizontal, hxb, hxe);
                    int hye = await askUser(i + " Health Y end", yui.Orientation.Horizontal, hxb, hxe);
                    if (hye < hyb) Util.Swap(ref hyb, ref hye);

                    var health = new Rect(hxb, hyb, hxe - hxb, hye - hyb);

                    res[i] = new PlayingScreen.Db.RD.CharacterTemplate { Health = health, Number = number };
                }

                foreach (var l in lines) l.Delete();
                tab.Status = prevStatus;

                return res;
            }


            List<yui.Rect> prevRects = new List<yui.Rect>() ;
            void displayCharTemplates(PlayingScreen.Db.RD.CharacterTemplate[] templates)
            {

                prevRects.ForEach(r => vp.Delete(r));
                prevRects.Clear();

                if (templates == null) return;
                Debug.Assert(templates.Length == 4);

                for (int i = 0; i < 4; i++)
                {
                    var character = templates[i];

                    var health = vp.CreateRect();
                    health.R = character.Health;
                    prevRects.Add(health);

                    var num = vp.CreateRect();
                    num.R = character.Number;
                    prevRects.Add(num);
                }
            }


            void filterHealth(ColorRange hg, ColorRange hr, Mat src, Mat dst, bool color=true)
            {
                using Mat hsv = src.CvtColor(ColorConversionCodes.BGR2HSV);
                using Mat thres = hsv.InRange(hg);
                using Mat thres2 = hsv.InRange(hr);
                if (color)
                {
                    Cv2.BitwiseOr(thres, thres2, thres);
                    using Mat cvt = thres.CvtColor(ColorConversionCodes.GRAY2BGRA);
                    cvt.CopyTo(dst);
                }
                else
                {
                    Cv2.BitwiseOr(thres, thres2, dst);
                }
            }

            void filterNum(double sMax, Mat src, Mat dst, bool color=true)
            {
                using Mat hsv = src.CvtColor(ColorConversionCodes.BGR2HSV);

                using Mat s = hsv.ExtractChannel(1);
                if (color)
                {
                    using Mat thres = s.Threshold(sMax, 255, ThresholdTypes.BinaryInv);
                    using Mat cvt = thres.CvtColor(ColorConversionCodes.GRAY2BGRA);
                    cvt.CopyTo(dst);
                }
                else
                {
                    Cv2.Threshold(s, dst, sMax, 255, ThresholdTypes.BinaryInv);
                }
            }


            PlayingScreen.Db.RD activeRD = null;
            Mat screenshot = null;
            Mat outputBuf = null;

            var healthGreen = sidebar.CreateSliders("green", db.CharFilter.HealthGreen, x=> {
                db.CharFilter.HealthGreen = x;
                updateHealthFilterPreview();
            }); 
            var healthRed = sidebar.CreateSliders("red", db.CharFilter.HealthRed, x => {
                db.CharFilter.HealthRed = x;
                updateHealthFilterPreview();
            });

            void updateHealthFilterPreview()
            {
                Debug.Assert(activeRD.Characters!=null);
                for (int i = 0; i < 4; i++)
                {
                    var character = activeRD.Characters[i];
                    var r = character.Health;
                    Mat src = screenshot[r];

                    if (db.CharFilter.HealthGreen is ColorRange hg && db.CharFilter.HealthRed is ColorRange hr)
                    {
                        filterHealth(hg,hr, src, outputBuf[r]);
                    }
                    else
                    {
                        src.CopyTo(outputBuf[r]);
                    }
                    img.Invalidate(r);
                }

            }

            void updateNumFilterPreview()
            {
                Debug.Assert(activeRD.Characters != null);

                for (int i = 0; i < 4; i++)
                {
                    var character = activeRD.Characters[i];
                    var r = character.Number;
                    Mat src = screenshot[r]; 



                    if (db.CharFilter.NumberSatMax is double sMax )
                    {
                        filterNum(sMax, src, outputBuf[r]);
                    }
                    else
                    {
                        src.CopyTo(outputBuf[r]);
                    }
                    img.Invalidate(r);
                }

            }

            bool filterAvail = false;
            void updateFilterAvail()
            {
                bool enable = filterAvail= outputBuf != null && activeRD != null && activeRD.Characters != null;
                numSatMax.Enabled = enable;
                for(int j=0;j<2;j++)
                for(int i = 0; i < 3; i++)
                {
                    healthGreen[j][i].Enabled = healthGreen[j][i].Enabled = enable;
                }

            }

            void setCharTemplates(PlayingScreen.Db.RD.CharacterTemplate[] templates)
            {
                Debug.Assert(activeRD != null);

                activeRD.Characters = templates;
                displayCharTemplates(activeRD.Characters);

                if (activeRD.Characters == null)
                {
                    clearBtn.Enabled = false;
                    refineBtn.Enabled = false;

                    //TODO unclear logic
                    outputBuf = screenshot.Clone();
                    img.Mat = outputBuf;


                    askCharacterTemplates().ContinueWith(x => setCharTemplates(x.Result));
                }
                else
                {
                    refineBtn.Enabled = true;
                    numSatMax.Enabled = true;
                    clearBtn.Enabled = true;
                }
                updateFilterAvail();
            }

            void setActiveRD(PlayingScreen.Db.RD rd)
            {
                activeRD = rd;
                updateFilterAvail();
                if (activeRD == null)
                {
                    clearBtn.Enabled = false;
                    refineBtn.Enabled = false;
                }
                else
                {
                    setCharTemplates(activeRD.Characters);
                }
            }

            Size? prevSize = null;
            void setScreenshot(Mat m)
            {
                screenshot = m;
                if (m == null)
                {
                    outputBuf = null;
                    img.Mat = null;
                    setActiveRD(null);
                }
                else
                {
                    outputBuf = screenshot.Clone();
                    img.Mat = outputBuf;

                    var size = m.Size();
                    if (size != prevSize)
                    {
                        prevSize = size;
                        if (!db.R.ContainsKey(size))
                            db.R[size] = new PlayingScreen.Db.RD();
                        activeRD = db.R[size];

                        setActiveRD(activeRD);
                    }
                }

                updateFilterAvail();
                if (filterAvail)
                {
                    updateNumFilterPreview();
                    updateHealthFilterPreview();
                }

            }
            
            

            refineBtn.Click += (s, e) =>
            {
                var filter = db.CharFilter;
                for (int i = 0; i < 4; i++)
                {
                    var character = activeRD.Characters[i];
                    if (filter.NumberSatMax is double sMax)
                    {
                        var number = character.Number;
                        using var filtered = new Mat();
                        filterNum(sMax, screenshot[number], filtered, color:false);
                        var blob = Util.FindBiggestBlob(filtered);
                        if (blob!=null && blob.Area > db.MinBlobArea)
                        {
                            character.Number = blob.Rect+number.TopLeft;
                        }
                    }

                    var health = character.Health;
                    if (db.CharFilter.HealthGreen is ColorRange hg && db.CharFilter.HealthRed is ColorRange hr)
                    {
                         var number = character.Health;
                        using var filtered = new Mat();
                        filterHealth(hg, hr, screenshot[number], filtered,color:false);
                        var blob = Util.FindBiggestBlob(filtered);
                        if (blob!=null && blob.Area > db.MinBlobArea)
                        {
                            character.Health = blob.Rect+health.TopLeft;
                        }
                    }
                }
                setCharTemplates(activeRD.Characters);
            };

            screenshotBtn.Click += async (s, e) =>
            {
                screenshotBtn.Enabled = false;
                var rect = await b.W.GetBoundsAsync();
                setScreenshot(await b.W.ScreenshotAsync(rect));
                screenshotBtn.Enabled = true;
            };

            clearBtn.Click += (s, e) =>
            {
                setCharTemplates(null);
            };

            numSatMax.VChanged += sax =>
            {
                db.CharFilter.NumberSatMax = sax;
                updateNumFilterPreview();
            };


            saveBtn.Click += async (s, e) =>
            {
                if (ui.Popup("Really save?", "Confirm", yui.PopupType.Confirm) == yui.PopupResult.Ok)
                {
                    await Task.Run(() => b.Db.SavePlayingScreenDb());
                    tab.Status = "Saved";
                }
            };

            setScreenshot(null);
            content.ResumeLayout();
        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}
