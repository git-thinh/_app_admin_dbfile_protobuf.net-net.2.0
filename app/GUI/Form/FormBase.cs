using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace app.GUI
{
    public class FormBase : Form
    {
        private Label lbl_Close;

        public FormBase()
            : this(App.Name, false)
        {
        }

        public FormBase(bool showCloseButton = false)
            : this(App.Name, showCloseButton)
        {
        }
        private Panel box;
        public FormBase(string title = App.Name, bool showCloseButton = false)
            : base()
        {
            Activated += (se, ev) =>
            {
                this.Left = (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2;
                this.Top = (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2;
            };

            Font = App.Font;
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = App.FormBorder;
            this.BackColor = App.ColorBorder;

            box = new Panel() { Dock = DockStyle.Top, BackColor = App.ColorBg, Height = 24, Padding = new Padding(0, 4, 3, 4), };

            Label lbl_Title = new Label() { Dock = DockStyle.Fill, Text = title, BackColor = App.ColorBg, ForeColor = Color.White, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            lbl_Title.MouseDown += FormMove_MouseDown;

            lbl_Close = new Label() { Visible = showCloseButton, Dock = DockStyle.Right, Text = "X", AutoSize = false, Width = 22, BackColor = Color.Red, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            lbl_Close.Click += (se, ev) => { this.Close(); };

            box.Controls.AddRange(new Control[] { lbl_Title, lbl_Close });

            this.Controls.Add(box);

        }

        public void PanelHeader_SendToBack() { box.SendToBack(); }


        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public void FormMove_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private List<IntPtr> HideControl = new List<IntPtr>();
        private List<ScrollBarHide> Hide_HORZ_VERT_0_1 = new List<ScrollBarHide>();
        public void HideScrollBar(IntPtr hideControl, ScrollBarHide hide_HORZ_VERT_0_1_3)
        {
            HideControl.Add(hideControl);
            Hide_HORZ_VERT_0_1.Add(hide_HORZ_VERT_0_1_3);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (HideControl.Count > 0)
                for (int k = 0; k < HideControl.Count; k++)
                    ShowScrollBar(HideControl[k], (int)Hide_HORZ_VERT_0_1[k], false);
        }
    }

    public enum ScrollBarHide
    { 
        SB_HORZ = 0,
        SB_VERT = 1,
        SB_BOTH = 3,
    }
}
