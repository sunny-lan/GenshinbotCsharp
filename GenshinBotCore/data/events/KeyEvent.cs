using genshinbot.automation.input;

namespace genshinbot.data.events
{
    public record KeyEvent
    {
        public Keys Key { get; init; }
        public bool Down { get; init; }
    }
}
