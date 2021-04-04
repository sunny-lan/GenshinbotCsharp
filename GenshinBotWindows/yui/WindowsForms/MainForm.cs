﻿using genshinbot.data;
using genshinbot.util;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
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
        public static YUI make()
        {
            var _f = new MainForm();
            Task.Run(() => Application.Run(_f));
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

        public void Popup(string message, string title = "")
        {
            MessageBox.Show(message, title);
        }

        public void GiveFocus(yui.Tab t)
        {
            tabs.SelectedTab = t as TabPage;
            Task.Run(async() =>
            {
                Color oldColor = statusStrip.BackColor;
                for (int i = 0; i < 3; i++)
                {
                    statusStrip.BackColor = Color.Red;
                    await Task.Delay(50);
                    statusStrip.BackColor = oldColor;
                    await Task.Delay(50);
                }
            });
        }
    }
}