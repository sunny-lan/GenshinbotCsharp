﻿using System;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class TablePanelContainer : TableLayoutPanel, yui.Container
    {
        private Flexbox flex;
        bool yui.Container.SupportsFlexbox => true;

        public TablePanelContainer() : base()
        {
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        }
        void yui.Container.SetFlex(Flexbox layout)
        {
            Invoke((MethodInvoker)delegate {
                _setFlex( layout);
            });
        }
        void _setFlex(Flexbox layout)
        {
            Debug.Assert(Controls.Count == 0, "Cannot set flex after controls added");
            this.flex = layout;
            //Debug.Assert(!layout.Wrap, "Wrap not supported");
            AutoScroll = flex.Scroll;
            if (layout.Direction == Orientation.Horizontal)
            {

                ColumnCount++;
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent,0));
            }
            else
            {
                RowCount++;
                RowStyles.Add(new RowStyle(SizeType.Percent,0));
            }
        }

        int totalWeight = 0;
        void yui.Container.SetFlex(object child, Flexbox.Item layout)
        {
            Invoke((MethodInvoker)delegate {
                _setFlex(child, layout);
            });
        }
        void _setFlex(object child, Flexbox.Item layout)
        {
            //TODO totalWeight is wrong if setflex called twice
            Debug.Assert(flex != null);

            int idx = Controls.IndexOf(child as Control);

            totalWeight += layout.Weight;
            if (flex.Direction == Orientation.Horizontal)
            {
                if (layout.Weight == 0)
                {
                    ColumnStyles[idx] = new ColumnStyle(SizeType.AutoSize);
                }
                else
                {
                    ColumnStyles[idx] = new ColumnStyle(SizeType.Percent, 100.0f * layout.Weight / totalWeight);

                }
            }
            else
            {
                if (layout.Weight == 0)
                {
                    RowStyles[idx] = new RowStyle(SizeType.AutoSize);
                }
                else
                {
                    RowStyles[idx] = new RowStyle(SizeType.Percent, 100.0f * layout.Weight / totalWeight);

                }
            }
        }
        private T add<T>(T c) where T : Control
        {
            Invoke((MethodInvoker)delegate
            {
                if (flex != null)
                {
                    if (flex.Direction == Orientation.Horizontal)
                    {
                        ColumnCount++;
                        ColumnStyles.Insert(ColumnCount-2,new ColumnStyle(SizeType.AutoSize));
                    }
                    else
                    {
                        RowCount++;
                        RowStyles.Insert(RowCount - 2,new RowStyle(SizeType.AutoSize));
                    }
                }
                c.Dock = DockStyle.Fill;
                dynamic d = c;
                d.AutoSize = true;
                Controls.Add(c);
            });
            return c;
        }


        public void ClearChildren()
        {
            Invoke((MethodInvoker)delegate
            {
                Controls.Clear();
            });
        }

        public void Delete(object btn)
        {
            if (btn is Control c)
                Invoke((MethodInvoker)delegate
                {
                    Controls.Remove(c);
                });
            else Debug.Assert(false);
        }

        public yui.Button CreateButton() => add(new Button());
        public yui.PropertyGrid CreatePropertyGrid() => add(new PropertyGrid());
        public yui.Slider CreateSlider() => add(new LabelledSlider());
        public yui.Container CreateSubContainer() => add(new TablePanelContainer());
        public yui.TreeView CreateTreeview() => add(new TreeView());
        public yui.Viewport CreateViewport() => add(new Viewport());

    }
}
