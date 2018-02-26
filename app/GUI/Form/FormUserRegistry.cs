using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace app.GUI
{
    public class FormUserRegistry : Form
    {
        public event OnExitEvent OnExit = null;
        public delegate void OnExitEvent();

        public delegate void EventSubmit(string user, string pass);
        public EventSubmit OnSubmit;

        public FormUserRegistry()
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            ShowInTaskbar = false;
            Padding = new Padding(2, 0, 2, 2);
            BackColor = App.ColorBg;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(200, 111);

            init_Control();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Label_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void init_Control()
        {
            Label lbl_Title = new Label() { Text = "REGISTRY SYSTEM ", Dock = DockStyle.Top, AutoSize = false, Width = 80, Height = 24, BackColor = App.ColorBg, ForeColor = Color.White, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            lbl_Title.MouseDown += Label_MouseDown;
            this.Controls.Add(lbl_Title);

            FlowLayoutPanel box = new FlowLayoutPanel() { Dock = DockStyle.Fill, BackColor = Color.White, FlowDirection = FlowDirection.LeftToRight };
            //box.MouseDown += Label_MouseDown;

            Label lbl_Username = new Label() { Text = "Username ", AutoSize = false, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleRight };
            Label lbl_Pass = new Label() { Text = "Password ", AutoSize = false, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleRight };

            TextBox txt_Username = new TextBox() { Width = 100, Text = "", BorderStyle = BorderStyle.FixedSingle };
            TextBox txt_Pass = new TextBox() { Width = 100, PasswordChar = '*', Text = "", BorderStyle = BorderStyle.FixedSingle };

            Label lbl_Space = new Label() { Text = "", AutoSize = false, Width = 10 }; 
             
            Button btn_Add = new Button() { Text = "REGISTRY", Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            Button btn_Close = new Button() { Text = "CLOSE", Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };

            box.Controls.AddRange(new Control[] { 
                lbl_Username,
                txt_Username,
                lbl_Pass,
                txt_Pass, 
                lbl_Space,
                btn_Add,
                btn_Close,
            }); 
            this.Controls.Add(box);
            box.BringToFront();
            btn_Add.Focus();

            btn_Close.Click += (se, ev) =>
            {
                if (OnExit != null) OnExit();
            };
            btn_Add.Click += (se, ev) =>
            {
                string user = txt_Username.Text.Trim(),
                    pass = txt_Pass.Text.Trim();

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) 
                {
                    MessageBox.Show("Username or password is empty","REGISTRY USER");
                    return;
                }

                this.Hide();
                if (OnSubmit != null) OnSubmit(user, pass);
            };
        }

    }
}
