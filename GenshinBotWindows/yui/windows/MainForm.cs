using System;
using genshinbot.util;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Vanara.PInvoke;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;

/// <summary>
/// provides implementation of gui using windows forms
/// </summary>
namespace genshinbot.yui.windows
{
    public partial class MainForm : Form, YUI
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;

                p.ExStyle |= (int)User32.WindowStylesEx.WS_EX_NOACTIVATE;

                return p;
            }
        }
        public static void Test()
        {
            var _f = new MainForm();
            yui.Tests.GenericTest(_f);
            Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled;
            Application.Run(_f);
        }
        public static MainForm make()
        {
            var _f = new MainForm();
            Task.Run(() => Application.Run(_f));
            var waiter = EventWaiter.Waiter<MainForm>();
            _f.Load += (s, e) => waiter.Item2(_f);
            waiter.Item1.Wait();
            return waiter.Item1.Result;
        }

        static Kernel32.SafeHINSTANCE mar = Kernel32.LoadLibrary("user32.dll");
        public MainForm()
        {
            TopMost = true;
            InitializeComponent();
            Load += MainForm_Load;
            del = HookProc1;
            
        }

        User32.HookProc del;
        private void MainForm_Load(object? sender, EventArgs e)
        {

            hookID = User32.SetWindowsHookEx(
                    User32.HookType.WH_KEYBOARD_LL,
                    del,
                    mar,
                    0);
            if (hookID.IsInvalid)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }

        }
        byte[] kbdSt = new byte[256];
        private IntPtr HookProc1(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (this.WindowState is not FormWindowState.Minimized)
                {
                    var converted = (User32.WindowMessage)wParam;
                    if (converted is
                        User32.WindowMessage.WM_KEYDOWN or
                        User32.WindowMessage.WM_KEYUP)
                    {
                        var focuseed = this.FindFocusedControl();
                        if (focuseed is Textbox t)
                        {
                            
                            var data = Marshal.PtrToStructure<User32.KBDLLHOOKSTRUCT>(lParam);
                            bool down = converted is User32.WindowMessage.WM_KEYDOWN;
                            kbdSt[data.vkCode] = (byte)(down ?0xff:0x00);
                            uint flag = 0x002C0001;
                            User32.SendMessage(t.Handle, (uint)wParam,
                                (IntPtr)data.vkCode, (IntPtr)flag);
                            //Console.WriteLine($"{(Keys)data.vkCode} {down}");
                            if (down)
                            {
                                User32.ToAscii(data.vkCode, data.scanCode, kbdSt, out var c,  0);
                                //Console.WriteLine((char)c);
                                User32.SendMessage(t.Handle, (uint)User32.WindowMessage.WM_CHAR, 
                                    (IntPtr)c,(IntPtr) flag);
                            }
                        }
                    }
                }
            }

            return User32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }




        public yui.Tab CreateTab()
        {
            Tab t = new Tab();
            Invoke((MethodInvoker)delegate { tabs.TabPages.Add(t); });
            return t;
        }

        public void RemoveTab(yui.Tab tab)
        {
            if (tab is TabPage pg)
                Invoke((MethodInvoker)delegate { tabs.TabPages.Remove(pg); });
            else Debug.Assert(false);
        }

        Tab lastSelected;

        public System.Func<bool> OnClose { get ; set ; }
        String REALMSG;
        private User32.SafeHHOOK hookID;

        private void tabs_Selected(object sender, TabControlEventArgs e)
        {

            if (e.TabPage is Tab tab)
            {

                statusMessage.Text = tab.Status;
                tab.StatusChanged = s => Invoke((MethodInvoker)delegate {
                    statusMessage.Text = s.Split('\n')[0] ;
                    REALMSG = s;
                }) ;
                if (lastSelected != null)
                    lastSelected.StatusChanged = null;
                lastSelected = tab;
            }
            else Debug.Assert(false);

        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(OnClose!=null)
                e.Cancel =await Task.Run( OnClose);
            if (!e.Cancel)
            {
                User32.UnhookWindowsHookEx(hookID);
            }
        }


        public void GiveFocus(yui.Tab t)
        {
            Action kkk = async () =>
            {
                tabs.SelectedTab = t as TabPage;
                Color oldColor = statusStrip.BackColor;
                for (int i = 0; i < 3; i++)
                {
                    statusStrip.BackColor = Color.Red;
                    await Task.Delay(50);
                    statusStrip.BackColor = oldColor;
                    await Task.Delay(50);
                }
            };
            tabs.BeginInvoke(kkk);
           
        }

        public PopupResult Popup(string message, string title = "", PopupType type = PopupType.Message)
        {
            MessageBoxButtons btns;
            switch (type)
            {
                case PopupType.Message:btns = MessageBoxButtons.OK;break;
                case PopupType.Confirm:btns = MessageBoxButtons.OKCancel;break;
                default:throw new NotSupportedException();
            }
            var res=MessageBox.Show(text:message,caption:title,buttons:btns);
            switch (res)
            {
                case DialogResult.OK: return PopupResult.Ok;
                case DialogResult.Cancel:return PopupResult.Cancel;
                default: throw new NotSupportedException();
            }

        }

        private void statusStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (REALMSG != "")
                Popup(REALMSG, "lONG");
        }
    }
}
