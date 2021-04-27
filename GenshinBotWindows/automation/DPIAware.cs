using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace genshinbot.automation
{
    static class DPIAware
    {/// <summary>
     /// Gets the predefined DPI_AWARENESS_CONTEXT handle for DPI unaware mode. These windows do not scale
     /// for DPI changes and are always assumed to have a scale factor of 100% (96 DPI). They will be automatically scaled by
     /// the system on any other DPI setting.
     /// </summary>
     /// <remarks>DPI_AWARENESS_CONTEXT values should never be compared directly. Instead, use AreDpiAwarenessContextsEqual function.</remarks>
        public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_UNAWARE = new DPI_AWARENESS_CONTEXT(new IntPtr(-1));

        /// <summary>
        /// Gets the predefined DPI_AWARENESS_CONTEXT handle for System aware mode. These windows do not scale for DPI changes.
        /// They will query for the DPI once and use that value for the lifetime of the process. If the DPI changes,
        /// the process will not adjust to the new DPI value. It will be automatically scaled up or down by the system
        /// when the DPI changes from the system value.
        /// </summary>
        /// <remarks>DPI_AWARENESS_CONTEXT values should never be compared directly. Instead, use AreDpiAwarenessContextsEqual function.</remarks>
        public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new DPI_AWARENESS_CONTEXT(new IntPtr(-2));

        /// <summary>
        /// Gets the predefined DPI_AWARENESS_CONTEXT handle for the Per Monitor mode. These windows check for the DPI when
        /// they are created and adjust the scale factor whenever the DPI changes. These processes are not automatically
        /// scaled by the system.
        /// </summary>
        /// <remarks>DPI_AWARENESS_CONTEXT values should never be compared directly. Instead, use AreDpiAwarenessContextsEqual function.</remarks>
        public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = new DPI_AWARENESS_CONTEXT(new IntPtr(-3));

        /// <summary>
        /// Gets the predefined DPI_AWARENESS_CONTEXT handle for Per Monitor v2 mode.
        /// Per Monitor v2 is an advancement over the original Per Monitor DPI awareness mode, which enables applications to access
        /// new DPI-related scaling behaviors on a per top-level window basis. Per Monitor v2 was made available in the
        /// Creators Update of Windows 10, and is not available on earlier versions of the operating system. The additional behaviors
        /// introduced are as follows:
        /// <list type="bullet">
        /// <item>Child window DPI change notifications - In Per Monitor v2 contexts, the entire window tree is notified of any DPI changes that occur.</item>
        /// <item>Scaling of non-client area - All windows will automatically have their non-client area drawn in a DPI sensitive fashion. Calls to EnableNonClientDpiScaling are unnecessary.</item>
        /// <item>Scaling of Win32 menus - All NTUSER menus created in Per Monitor v2 contexts will be scaling in a per-monitor fashion.</item>
        /// <item>Dialog Scaling - Win32 dialogs created in Per Monitor v2 contexts will automatically respond to DPI changes.</item>
        /// <item>Improved scaling of comctl32 controls - Various comctl32 controls have improved DPI scaling behavior in Per Monitor v2 contexts.</item>
        /// <item>Improved theming behavior - UxTheme handles opened in the context of a Per Monitor v2 window will operate in terms of the DPI associated with that window.</item>
        /// </list>
        /// </summary>
        /// <remarks>DPI_AWARENESS_CONTEXT values should never be compared directly. Instead, use AreDpiAwarenessContextsEqual function.</remarks>
        public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new DPI_AWARENESS_CONTEXT(new IntPtr(-4));

        /// <summary>
        /// DPI unaware with improved quality of GDI-based content. This mode behaves similarly to <see cref="DPI_AWARENESS_CONTEXT_UNAWARE" />,
        /// but also enables the system to automatically improve the rendering quality of text and other GDI-based primitives when
        /// the window is displayed on a high-DPI monitor.
        /// </summary>
        /// <remarks>
        /// <see cref="DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED" /> was introduced in the October 2018 update
        /// of Windows 10 (also known as version 1809).
        /// For more details, see <see href="https://blogs.windows.com/buildingapps/2017/05/19/improving-high-dpi-experience-gdi-based-desktop-apps/#Uwv9gY1SvpbgQ4dK.97">Improving the high-DPI experience in GDI-based Desktop apps</see>.
        /// </remarks>
        public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = new DPI_AWARENESS_CONTEXT(new IntPtr(-5));
     
        public static void Set(SHCore.PROCESS_DPI_AWARENESS aware)
        {
            SHCore.SetProcessDpiAwareness(aware).ThrowIfFailed();
        }

        public static void Use(DPI_AWARENESS_CONTEXT context, Action a)
        {
            var prev = User32.SetThreadDpiAwarenessContext(context);
            a();
            User32.SetThreadDpiAwarenessContext(prev);
        }
        public static T Use<T>(DPI_AWARENESS_CONTEXT context, Func<T> a)
        {
            var prev = User32.SetThreadDpiAwarenessContext(context);
            var ret=a();
            User32.SetThreadDpiAwarenessContext(prev);
            return ret;
        }
    }
}
