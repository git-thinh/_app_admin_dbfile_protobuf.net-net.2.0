using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace app.GUI
{
    public class SystemTray : ApplicationContext
    {
        public event OnExitEvent OnExit = null;
        public delegate void OnExitEvent();

        public event OnClickEvent OnClick = null;
        public delegate void OnClickEvent();

        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu; 
        private string Name = "";

        public SystemTray(string name)
        {
            Name = name;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();
            TrayIcon.Text = Name;

            ///////////////////////////////////////////////////////////////////////////
            // Set m_icon from resource
            string resourceName = "icon.ico";
            var assembly = Assembly.GetExecutingAssembly();
            resourceName = typeof(App).Namespace + "." + resourceName.Replace(" ", "_")
                .Replace("\\", ".").Replace("/", ".");
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                //The icon is added to the project resources.
                //Here I assume that the name of the file is 'TrayIcon.ico' 
                TrayIcon.Icon = new Icon(stream); //Properties.Resources.TrayIcon;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.MouseClick += (se, ev) =>
            {
                switch (ev.Button)
                {
                    case MouseButtons.Right:
                        break;
                    case MouseButtons.Left:
                        if (OnClick != null) OnClick();
                        break;
                }
            };

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            ToolStripMenuItem mn_close = new ToolStripMenuItem() { Text = "Close " + Name };
            mn_close.Click += new EventHandler(this.CloseMenuItem_Click); 

            // 
            // TrayIconContextMenu
            // 
            TrayIconContextMenu.SuspendLayout();
            TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {  mn_close});
            TrayIconContextMenu.Name = "TrayIconContextMenu";
            TrayIconContextMenu.Size = new Size(153, 70);


            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        public void Hide()
        {
            TrayIcon.Visible = false;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (OnExit != null) OnExit();
        }
    }
}
