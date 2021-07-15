using System;
using genshinbot.util;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace genshinbot.yui.windows
{
    /// <summary>
    /// provides implementation of gui using windows forms
    /// </summary>
    public partial class MainForm : Form, YUI
    {
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

        public MainForm()
        {
            InitializeComponent();
            
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

        private void tabs_Selected(object sender, TabControlEventArgs e)
        {

            if (e.TabPage is Tab tab)
            {

                statusMessage.Text = tab.Status;
                tab.StatusChanged = s => statusMessage.Text = s;
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

    }
}
