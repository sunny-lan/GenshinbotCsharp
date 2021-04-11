using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    interface BaseContainerImpl :yui.Container
    {
        public T add<T>(T c) where T : Control;
        yui.Button yui.Container.CreateButton() => add(new Button());
        yui.PropertyGrid yui.Container.CreatePropertyGrid() => add(new PropertyGrid());
        yui.Slider yui.Container.CreateSlider() => add(new LabelledSlider());
        yui.Container yui.Container.CreateSubContainer() => add(new TablePanelContainer());
        yui.TreeView yui.Container.CreateTreeview() => add(new TreeView());
        yui.Viewport yui.Container.CreateViewport() => add(new Viewport());

        yui.Expander yui.Container.CreateExpander() => add(new Expander());

        yui.Label yui.Container.CreateLabel() => add(new Label());
    }
}
