using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace app.GUI
{
    public partial class ucModelTitle : UserControl
    {
        public ucModelTitle()
        {
            Label lbl_Name = new Label() { Left = 4, Top = 0, Width = 80, Text = "Name", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Type = new Label() { Left = 88, Top = 0, Width = 60, Text = "Type", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Auto = new Label() { Left = 152, Top = 0, Width = 36, Text = "Auto", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Control = new Label() { Left = 192, Top = 0, Width = 80, Text = "Control", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            Label lbl_LinkType = new Label() { Left = 276, Top = 0, Width = 84, Text = "Join Value", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_LinkModel = new Label() { Left = 362, Top = 0, Width = 100, Text = "Join Model", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_LinkField = new Label() { Left = 464, Top = 0, Width = 100, Text = "Join Field", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_LinkView = new Label() { Left = 568, Top = 0, Width = 100, Text = "Join View", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            Label lbl_Index = new Label() { Left = 670, Top = 0, Width = 38, Text = "Index", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Null = new Label() { Left = 710, Top = 0, Width = 30, Text = "Null", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            this.Controls.AddRange(new Control[] { lbl_Name, lbl_Type, lbl_Auto, lbl_Control,
                lbl_LinkType,lbl_LinkModel, lbl_LinkField,
                lbl_LinkView,
                lbl_Index, lbl_Null,
            });
        }
    }
}
