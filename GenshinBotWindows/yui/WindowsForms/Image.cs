using genshinbot.data;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
        class Image : PictureBox, yui.Image, ViewportComponent
        {
            Mat _img;
            Bitmap bmp;
            private OpenCvSharp.Point _pos;
            private Transformation _t=new Transformation();
            

            /// <summary>
            /// must not be disposed before the class is disposed
            /// </summary>
            public Mat Mat
            {
                get => _img;
                set
                {
                    _img = value;
                    bmp = value.ToBmpFast();
                    Image = bmp;
                    Recalc();
                }
            }
            public OpenCvSharp.Point TopLeft
            {
                get => _pos;
                set
                {
                    _pos = value;
                    Recalc();
                }
            }

            private void Recalc()
            {
                if (_img != null)
                {
                    this.Bounds = _t.Transform(new Rect2d(_pos, _img.Size().cvt())).round().Sys();
                }
            }

            public Image():base()
            {
                SizeMode = PictureBoxSizeMode.StretchImage;
                
                
            }



            public void SetTransform(Transformation t)
            {
                _t = t;
                Recalc();
            }

            //TODO
            public void Invalidate(OpenCvSharp.Rect r)
            {
                throw new NotImplementedException();
            }
        }
}
