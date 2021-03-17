using GenshinbotCsharp.data;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{
    class ImageDrawable : Drawable,yui.Image
    {
        Mat _img;
        Bitmap bmp;
        private OpenCvSharp.Point _pos;
        private Viewport parent;

        public ImageDrawable(Viewport parent)
        {
            this.parent = parent;
        }


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
                parent.Invalidate();
            }
        }
        public OpenCvSharp.Point TopLeft
        {
            get => _pos;
            set
            {
                _pos = value;
                parent.Invalidate();
            }
        }

        public void Invalidate()
        {
            parent.Invalidate();
        }

        public void Invalidate(OpenCvSharp.Rect r)
        {
            throw new NotImplementedException();
        }

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(bmp, _pos.Sys());
        }
    }
}
