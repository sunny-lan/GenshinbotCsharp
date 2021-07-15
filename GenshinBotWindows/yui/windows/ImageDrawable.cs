using genshinbot.data;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class ImageDrawable : Drawable,yui.Image
    {
        Mat ?_img;
        Bitmap? bmp;
        private OpenCvSharp.Point _pos;
        private Viewport parent;

        public ImageDrawable(Viewport parent)
        {
            this.parent = parent;
        }


        /// <summary>
        /// must not be disposed before the class is disposed
        /// </summary>
        public Mat? Mat
        {
            get => _img;
            set
            {
                if(value is null)
                {
                    _img = null;
                    bmp = null;
                }
                else if (value != _img)
                {
                    _img = value;
                    bmp = value?.ToBmpFast();
                }
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

        public event Action<MouseEvent>? MouseEvent;

        public void Invalidate()
        {
            parent.Invalidate();
        }

        public void Invalidate(OpenCvSharp.Rect r)
        {
            parent.Invalidate(r);
        }

        public void OnPaint(PaintEventArgs e)
        {
            if(bmp!=null)
                    e.Graphics.DrawImage(bmp, _pos.Sys());
        }

        public bool Parent_MouseEvent(MouseEvent obj)
        {
            if (_img is null) return false;
            var r = new OpenCvSharp.Rect(_pos, _img.Size());
            if (r.Contains(obj.Location))
            {
                var tmp = obj;
                tmp.Location -= r.TopLeft;
                MouseEvent?.Invoke(tmp);
                return true;
            }
            return false;
        }
    }
}
