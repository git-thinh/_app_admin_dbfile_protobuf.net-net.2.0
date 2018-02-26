using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace app.GUI
{
   public class FormNotification: Form
    { 
        private  Label label1;
        private  Label lblTitle;
        private  Label lblBody;
       
        private void InitializeComponent()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNotification));
            this.label1 = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblBody = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.OrangeRed;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 2);
            this.label1.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(0, 2);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(284, 26);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Message";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.Padding = new Padding(10, 0, 0, 0);
            // 
            // lblBody
            // 
            this.lblBody.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBody.Location = new System.Drawing.Point(0, 28);
            this.lblBody.Name = "lblBody";
            this.lblBody.Size = new System.Drawing.Size(284, 68);
            this.lblBody.TabIndex = 2;
            this.lblBody.Text = "Here";
            this.lblBody.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // f_notiMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(284, 96);
            this.Controls.Add(this.lblBody);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "fNotiMsg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Notification";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.fNoti_Load);
            this.ResumeLayout(false);

        }

        private static List<FormNotification> lsNoti = new List<FormNotification>() { };
        private System.Windows.Forms.Timer lifeTimer;
        private int duration = 3000; 

        public FormNotification()
        {
            InitializeComponent();
            //this.Text = main.Name;
        }

        public FormNotification(string body, int duration_ = 3000)
        {
            InitializeComponent();
            //this.Text = main.Name;

            lblBody.Text = body;
            duration = duration_; 
        }

        public FormNotification( string body, string title,int duration_ = 3000)
        {
            InitializeComponent();

            if(!string.IsNullOrEmpty(title)) lblTitle.Text = title;
            lblBody.Text = body;
            duration = duration_; 
        }

        private void fNoti_Load(object sender, EventArgs e)
        {
            //lblTitle.Text = lblTitle.Text + lsNoti.Count.ToString();//

            this.FormClosed+=fNoti_FormClosed;
            lblBody.Click += label_Click;
            lblTitle.Click += label_Click;

            // Display the form just above the system tray.
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width,
                                      Screen.PrimaryScreen.WorkingArea.Height - Height);

            // Move each open form upwards to make room for this one
            foreach (FormNotification openForm in lsNoti)
            {
                openForm.Top -= Height + 7;
            }

            lsNoti.Add(this);

            if (duration > 0)
            {
                this.lifeTimer = new System.Windows.Forms.Timer();
                this.lifeTimer.Tick += lifeTimer_Tick;
                lifeTimer.Interval = duration;
                lifeTimer.Start();
            }
        }

        private void lifeTimer_Tick(object sender, EventArgs e)
        {
            this.lifeTimer.Stop();
            this.Close();
        }

        private void fNoti_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Move down any open forms above this one
            foreach (FormNotification openForm in lsNoti)
            {
                if (openForm == this)
                {
                    // Remaining forms are below this one
                    break;
                }
                openForm.Top += Height + 7;
            }

            lsNoti.Remove(this);
        }

        private void label_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
