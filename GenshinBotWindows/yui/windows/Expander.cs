using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Expander : TableLayoutPanel, yui.Expander
    {
        public Expander() : base()
        {
            RowCount = 2;
            AutoSize = true;

             label = new Label();
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.BackColor = SystemColors.ActiveBorder;
            label.Dock = DockStyle.Fill;
            Controls.Add(label);

            bool expanded = true;
            _content = new TablePanelContainer();
            _content.AutoSize = true;
            _content.Dock = DockStyle.Fill;
            Controls.Add(_content);

            label.Click += delegate
            {
                expanded = !expanded;
                if (expanded)
                {
                    RowCount = 2;
                    label.BackColor = SystemColors.ActiveBorder;
                    Controls.Add(_content);   
                }
                else
                {
                    RowCount = 1;
                    label.BackColor = SystemColors.ActiveCaption;
                    Controls.Remove(_content);
                }
            };


        }
        TablePanelContainer _content;
        private Label label;

        Container yui.Expander.Content => _content;

        public string Label
        {
            get => label.Text;set => label.Text=value;
        }
    }
}
