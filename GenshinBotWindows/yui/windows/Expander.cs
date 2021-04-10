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
        bool expanded = true;
        public Expander() : base()
        {
            RowCount = 2;
            AutoSize = true;
            SuspendLayout();

            label = new Label();
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.BackColor = SystemColors.ActiveBorder;
            label.Dock = DockStyle.Fill;
            label.AutoSize = true;
            Controls.Add(label);
            RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _content = new TablePanelContainer();
            _content.AutoSize = true;
            _content.Dock = DockStyle.Fill;
            Controls.Add(_content);
            RowStyles.Add(new RowStyle(SizeType.AutoSize));

            label.Click += delegate
            {
                Expanded = !Expanded;
                
            };

            ResumeLayout();


        }
        TablePanelContainer _content;
        private Label label;

        Container yui.Expander.Content => _content;

        public string Label
        {
            get => label.Text; set => label.Text = value;
        }
        public bool Expanded
        {
            get => expanded; set
            {
                expanded = value;
                if (expanded)
                {
                    label.BackColor = SystemColors.ActiveCaption;
                    RowStyles[1]=(new RowStyle(SizeType.AutoSize));
                }
                else
                {
                    label.BackColor = SystemColors.ActiveBorder;
                    RowStyles[1] = (new RowStyle(SizeType.Absolute,0));
                }
            }
        }
    }
}
