namespace GenshinbotCsharp.yui.WindowsForms
{
    class PropertyGrid :System.Windows.Forms.PropertyGrid, yui.PropertyGrid
    {
        public PropertyGrid():base()
        {
            Size = new System.Drawing.Size(500, 500);//TODO
        }
    }
}
