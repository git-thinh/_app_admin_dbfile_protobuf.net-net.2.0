using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sano.PersonalProjects.ColorPicker.Controls;
using System.Drawing;

namespace app.GUI
{
    public class FormColorPicker : FormBase
    {
        public FormColorPicker(bool showClose = false)
            : base("COLOR PICKER", showClose)
        {
            FormBorderStyle = FormBorderStyle.None;
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(576, 301);

            ColorPanel colorPanel1 = new ColorPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                AllowDrop = true,
                Location = new System.Drawing.Point(2, 14),
                Size = new System.Drawing.Size(572, 272),
                TabIndex = 0,
            };
            this.Controls.Add(colorPanel1);
            colorPanel1.BringToFront();
        }


    }
}
