using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{
    public partial class TreeView : System.Windows.Forms.TreeView,yui.TreeView
    {
        class Node : TreeNode, yui.TreeView.Node
        {
            public OpenCvSharp.Scalar Color {
                get => this.ForeColor.cv3();
                set => this.ForeColor = value.SysBgr255();
            }

            public event EventHandler DoubleClick;
            public event EventHandler Selected;
            public event EventHandler Deselected;

            public Node():base()
            {
            }

            public yui.TreeView.Node CreateChild()
            {
                var nd = new Node();
                Nodes.Add(nd);
                return nd;
            }



            internal void  OnSelect(EventArgs e) {
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
                Nodes.Clear();
            }

            public void Invalidate()
            {
               
            }
        }

        public TreeView():base()
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

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            if(e.Node is Node n)
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
            Nodes.Add(nd);
            return nd;
        }
    }
}
