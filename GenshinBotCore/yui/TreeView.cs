using OpenCvSharp;
using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface TreeView
    {
        public interface Node
        {
            Node CreateChild();
            event EventHandler DoubleClick;
            event EventHandler Selected;
            event EventHandler Deselected;
            string Text { get; set; }
            Scalar Color { get; set; }
            void Delete(Node child);
            void ClearChildren();
            void Invalidate();
        }

        public Node CreateNode();
        void Delete(Node child);
        void ClearChildren();


        public void BeginUpdate();
        public void EndUpdate();

        public void GiveFocus(Node n);
    }
}
