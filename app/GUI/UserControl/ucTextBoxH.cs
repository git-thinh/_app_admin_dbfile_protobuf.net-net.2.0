using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace app.GUI
{
    public partial class ucTextBoxH : UserControl
    {
        private readonly TextBox txt;
        private readonly Label lbl;

        public ucTextBoxH(int _width = 80)
        {
            lbl = new Label()
           {
               Height = 18, 
               Dock = DockStyle.Top,
               TextAlign = ContentAlignment.BottomLeft,
           };

            txt = new TextBox()
            {
                Dock = DockStyle.Fill,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            };

            this.ClientSize = new Size(_width, 40);
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.AddRange(new Control[] { txt, lbl });
        }

        public bool OnlyInputNumber0To9
        {
            set
            {
                if (value)
                    txt.KeyPress += (se, ev) =>
                    {
                        if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                        {
                            ev.Handled = true;
                        }
                    };
            }
        }

        public string Title
        {
            set { lbl.Text = value; }
            get { return lbl.Text; }
        }

        public HorizontalAlignment TextAlign
        {
            set { txt.TextAlign = value; }
            get { return txt.TextAlign; }
        }

        public string WaterMark
        {
            set;
            get;
        }

        public override string Text
        {
            set { txt.Text = value; }
            get { return txt.Text; }
        }

    }
}
