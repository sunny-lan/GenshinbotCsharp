using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.yui
{
    public class _Expander
    {
        public Container Content { get; private set; }
        public bool Expanded
        {
            get => expanded; set {
                expanded = value;
                if (expanded)
                {
                    
                }
                else
                {

                }
            }
        }
        public Action<bool> OnExpandChanged { get; set; }

        private Container parent;
        private Button btn;

        private bool expanded;

        public _Expander(Container c)
        {
            this.parent = c;
            btn = c.CreateButton();
            Content = c.CreateSubContainer();
            btn.Click += Btn_Click;
            OnExpandChanged = x => this.Expanded = x;
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            OnExpandChanged(!expanded);
        }

        public void Delete()
        {
            parent.Delete(btn);
            parent.Delete(Content);
        }
    }
}
