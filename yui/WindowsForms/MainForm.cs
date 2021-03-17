using GenshinbotCsharp.data;
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

        public MainForm()
        {
            InitializeComponent();
        }

        public yui.Tab CreateTab()
        {
            Tab t = new Tab();
            this.tabs.TabPages.Add(t);
            return t;
        }
    }
}
