using GenshinbotCsharp.data;
using GenshinbotCsharp.util;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
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
            Application.Run(_f);
        }
        public static YUI make()
        {
            var _f = new MainForm();
            Task.Run(()=>Application.Run(_f));
            var waiter = EventWaiter.Waiter<YUI>();
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

        private void tabs_Selected(object sender, TabControlEventArgs e)
        {
           if(e.TabPage is Tab tab)
            {
                statusStrip.Text = tab.Status;
            }
            else Debug.Assert(false);

        }
    }
}
