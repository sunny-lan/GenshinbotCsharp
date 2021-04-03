using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    public partial class TreeView : System.Windows.Forms.TreeView, yui.TreeView
    {
        class Node : TreeNode, yui.TreeView.Node
        {
            public OpenCvSharp.Scalar Color
            {
                get => this.ForeColor.cv3();
                set => this.ForeColor = value.SysBgr255();
            }

            public event EventHandler DoubleClick;
            public event EventHandler Selected;
            public event EventHandler Deselected;

            string yui.TreeView.Node.Text
            {
                get => base.Text; 
                set=> TreeView.Invoke((MethodInvoker)delegate { base.Text = value; });
            }

            public Node() : base()
            {
            }

            public yui.TreeView.Node CreateChild()
            {
                var nd = new Node();
                TreeView.Invoke((MethodInvoker)delegate
                {
                    Nodes.Add(nd);
                });
                return nd;
            }



            internal void OnSelect(EventArgs e)
            {
                Selected?.Invoke(this, e);
            }
            internal void OnDoubleClick(EventArgs e)
            {
                DoubleClick?.Invoke(this, e);
            }

            internal void OnDeselect(EventArgs e)
            {
                Deselected?.Invoke(this, e);
            }
            public void ClearChildren()
            {
                TreeView.Invoke((MethodInvoker)delegate
                {
                    Nodes.Clear();
                });
            }

            public void Invalidate()
            {

            }

            public void Delete(yui.TreeView.Node child)
            {
                TreeView.Invoke((MethodInvoker)delegate
                {
                    Nodes.Remove(child as TreeNode);
                });
            }
        }


        public TreeView() : base()
        {
            InitializeComponent();
            //TODO
            Size = new Size(500, 500);
            

        }

        Node prevSelected;

        protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseDoubleClick(e);
            if (e.Node is Node n)
            {
                n.OnDoubleClick(e);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            prevSelected?.OnDeselect(e);
            prevSelected = null;
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            if (e.Node is Node n)
            {
                prevSelected?.OnDeselect(e);
                prevSelected = n;
                n.OnSelect(e);
                
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);
        }

        public yui.TreeView.Node CreateNode()
        {
            var nd = new Node();
            Invoke((MethodInvoker)delegate
            {
                Nodes.Add(nd);
            });
            return nd;
        }

        public void Delete(yui.TreeView.Node child)
        {
            Invoke((MethodInvoker)delegate
            {
                Nodes.Remove(child as TreeNode);
            });
        }

        public void ClearChildren()
        {
            Invoke((MethodInvoker)delegate
            {
                Nodes.Clear();
            });
        }
        void yui.TreeView.BeginUpdate()
        {
            Invoke((MethodInvoker)delegate { base.BeginUpdate(); });
        }
        void yui.TreeView.EndUpdate()
        {
            Invoke((MethodInvoker)delegate { base.EndUpdate(); });
        }

        public void GiveFocus(yui.TreeView.Node n)
        {
            var x = n as TreeNode;
            Invoke((MethodInvoker)delegate { SelectedNode = x; });
        }
    }
}
