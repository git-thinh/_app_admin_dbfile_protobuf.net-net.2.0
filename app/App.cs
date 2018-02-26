using System;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using app.GUI;
using app.Core;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using app.Model;
using System.Management;
using Microsoft.VisualBasic.ApplicationServices;

namespace app
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    class App
    {
        #region [ === CONFIG APP: FONT, COLOR === ]

        public static string UserCurrent = "";

        public const int col_Left_Width = 99;
        public const int col_Splitter_Width = 1;
        public const int Width = 999;
        public const string Name = "TEST SYSTEM";
        public static Font Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        public static Padding FormBorder = new Padding(1, 1, 1, 1);
        public static Color ColorBorder = SystemColors.ControlDarkDark;
        public static Color ColorBg = Color.Black;
        public static Color ColorControl = Color.LightGray;

        #endregion

        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(App).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        #region [ === DB, FORM === ]

        ////////////////////////////////////////////
        ////static Log log;
        ////static HostListener host;  
        static DataHost db;

        ////////////////////////////////////////
        static SystemTray icon_tray;
        static FormNotification fm_noti;
        static FormLogger fm_log;
        static FormLogin fm_login;
        static FormDB fm_DB;
        static FormELSearch fm_search;
        static FormGecko fm_gecko;

        public class demo
        {
            public string time { set; get; }
        }

        #endregion

        #region [ === CASSINI === ]

        /// <summary>
        /// Check on path folder current only run App once
        /// </summary>
        private static void close_CassiniRunning()
        {
            try
            {
                // Get all process running
                var wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process";
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                using (var results = searcher.Get())
                {
                    var ps = (from p in Process.GetProcesses()
                              join mo in results.Cast<ManagementObject>()
                              on p.Id equals (int)(uint)mo["ProcessId"]
                              select new
                              {
                                  Id = p.Id,
                                  Name = p.ProcessName,
                                  Path = (string)mo["ExecutablePath"],
                              })
                                .Where(x =>
                                    !string.IsNullOrEmpty(x.Name)
                                    && !string.IsNullOrEmpty(x.Path)
                                    && x.Path.Contains("cassini.exe"))
                                .ToArray();

                    // If exist other application running
                    if (ps.Length > 1)
                    {
                        //// If click open second show message exit
                        // if (len == 2){
                        //    MessageBox.Show("app running -> exit()"); exit(); }
                        
                        foreach (var it in ps)
                        {
                            Process p = Process.GetProcessById(it.Id);
                            p.Kill();
                        }
                    }
                }
            }
            catch(Exception ex)
            { 
            }
        }//end func

        private static bool m_CassiniInited = false;
        private static Process processCassini;
        private static Thread threadCassini;
        static void run_Cassini()
        {
            close_CassiniRunning();
            threadCassini = new Thread(() =>
            {
                string fi = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Service\cassini.exe");
                processCassini = new Process();
                processCassini.StartInfo.FileName = fi;
                processCassini.StartInfo.Arguments = @"61422 D:\_code\w";
                processCassini.StartInfo.UseShellExecute = false;
                processCassini.StartInfo.RedirectStandardOutput = true;
                processCassini.StartInfo.RedirectStandardError = false;

                processCassini.StartInfo.CreateNoWindow = true;
                processCassini.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                processCassini.OutputDataReceived += (se, ev) =>
                {
                    string data = ev.Data;
                    //MessageBox.Show(data);
                    if (data == "[BEGIN]")
                    {
                        //show_Form(new FormGecko());
                        m_CassiniInited = true;
                    }
                };

                //* Start process and handlers
                processCassini.Start();
                processCassini.BeginOutputReadLine();
                processCassini.WaitForExit();
                //process.Close();
                ;
            });
            threadCassini.Start();
        }

        #endregion

        public static void Start()
        {
            run_Cassini();

            //string htm = File.ReadAllText("demo.html"); 

            //List<string> listMp3 = new List<string>();
            //var ps2 = Regex.Matches(htm, "http(.+?)mp3", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //foreach (Match mi in ps2)
            //{
            //    string img = mi.ToString();
            //    listMp3.Add(img);
            //}





            Application.EnableVisualStyles();



            ////Func<string, DateTime> func = delegate (string item)
            ////{
            ////    return DateTime.ParseExact(item, "yyMMddHHmmss", null);
            ////};

            ////demo[] dt = new demo[] { new demo() { time = DateTime.Now.ToString("yyMMddHHmmss") } };

            //////var l0 = dt.AsQueryable().Select(typeof(Result), @"new @out (@0(it.time) as data)", func);

            ////var l0 = dt.AsQueryable().Select(@"new (@0(it.time) as data)", func);

            //////var l1 = (IList)typeof(List<>).MakeGenericType(l0.ElementType).GetConstructor(Type.EmptyTypes).Invoke(null);
            //////foreach (var elem in l0) l1.Add(elem);

            ////IList l2 = new List<object>();
            ////foreach (var elem in l0) l2.Add(elem);



            db = new DataHost();

            //////////////////////////////////////////////
            ////RuntimeTypeModel.Default.Add(typeof(Msg), false).SetSurrogate(typeof(MsgSurrogate));

            ////////////////////////////////////////////
            ////////log = new Log();  
            ////////host = new HostListener(log);
            ////host.Start();

            //////////////////////////////////////////////
            noti_Init();
            icon_tray = new SystemTray("Host");
            fm_noti = new FormNotification();
            fm_log = new FormLogger();
            fm_login = new FormLogin();
            fm_search = new FormELSearch(db);
            fm_login.OnExit += () => Exit();

            icon_tray.OnClick += () =>
            {
                if (string.IsNullOrEmpty(UserCurrent))
                    show_Form(fm_login);
                else
                    show_Form(fm_DB);
            };
            icon_tray.OnExit += () => Exit();

            //////////////////////////////////////////

            db.OnOpen += (string[] a) =>
            {
                if (db.Open)
                {
                    fm_DB = new FormDB(db);
                    fm_DB.OnExit += () => Exit();


                    show_Form(fm_DB);

                    //show_Form(new FormGecko());

                    //show_Form(new FormELSearch(db));

                    //show_Form(new FormDataSplitConfig(db, "http://langmaster.edu.vn/bi-quyet-a15i1040.html"));

                    //show_Form(new FormModelAdd(db));
                    //show_Form(new FormModelEdit(db, "USER"));

                    //show_Form(new FormModelEdit(db, "JOIN1"));
                    //show_Form(new FormItemAdd(db, "JOIN1"));

                    ////show_Form(new FormColorPicker(true));
                    //show_Form(new FormLookupItem(db));

                    //init_Login();
                }
                else
                {
                    MessageBox.Show("SYSTEM CAN NOT OPEN. PLEASE CHECK DATA FILE.", "SYSTEM OPEN");
                }
            };
            db.Start();

            while (m_CassiniInited == false) {; }

            fm_gecko = new FormGecko();
            show_Form(fm_gecko);

            Application.Run(icon_tray);
        }

        static void init_Login()
        {
            fm_login.OnLogin += (user, pass) =>
            {
                LOGIN_STATUS login = db.Login(user, pass);
                switch (login)
                {
                    case LOGIN_STATUS.MODEL_USER_IS_EMPTY:
                        var f = new FormUserRegistry();
                        f.OnSubmit += (user_new, pass_new) =>
                        {
                            object rs = db.AddItem(new USER() { FULLNAME = "", PASSWORD = pass_new, USERNAME = user_new });
                            if (rs != null)
                            {
                                UserCurrent = user_new;
                                f.Close();
                                fm_DB.ShowUser(UserCurrent);
                                show_Form(fm_search);
                            }
                            else
                            {
                                MessageBox.Show("Registry user fail, " + rs.ToString());
                                f.ShowDialog();
                            }
                        };
                        f.ShowDialog();
                        break;
                    case LOGIN_STATUS.USERNAME_PASS_WRONG:
                        fm_login.ShowMessage("User or pass wrong");
                        break;
                    case LOGIN_STATUS.LOGIN_SUCCESSFULLY:
                        UserCurrent = user;
                        fm_DB.ShowUser(UserCurrent);
                        show_Form(fm_search);
                        break;
                }
            };
            show_Form(fm_login);
        }

        #region [ === EXIT, SHOW_FORM, SHOW_NOTIFICATION === ]

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        private static void Exit()
        {
            DialogResult ok = MessageBox.Show("Application will exit ?", "Are you sure ?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2);
            if (ok == DialogResult.Yes)
            {
                var _memoryService = Gecko.Xpcom.GetService<Gecko.nsIMemory>("@mozilla.org/xpcom/memory-service;1");
                _memoryService.HeapMinimize(false);

                fm_gecko.Close();
                 
                processCassini.Close();
                threadCassini.Interrupt();
                threadCassini.Abort();
                close_CassiniRunning();

                if (db != null) db.Close();

                if (fm_DB != null) fm_DB.Close();
                if (fm_login != null) fm_login.Close();
                if (fm_log != null) fm_log.Close();
                if (fm_noti != null) fm_noti.Close();

                if (icon_tray != null) icon_tray.Hide();

                Thread.Sleep(300);

                int pi = Process.GetCurrentProcess().Id;
                Process p = Process.GetProcessById(pi);
                p.Kill();
            }
        }

        public static void show_Form(Form fm)
        {
            fm.Show();
            fm.Left = (Screen.PrimaryScreen.WorkingArea.Width - fm.Width) / 2;
            fm.Top = (Screen.PrimaryScreen.WorkingArea.Height - fm.Height) / 2;
        }

        public static void show_FormDialog(Form fm)
        {
            fm.ShowDialog();
        }

        // very simple method to create new forms, controls ... is to use Invoke of this control
        private static Control _invoker;
        public static void show_Notification(string msg, int duration_ = 0)
        {
            FormNotification form = new FormNotification(msg, duration_);
            _invoker.Invoke((MethodInvoker)delegate ()
            {
                form.Show();
            });
        }

        static void noti_Init()
        {
            _invoker = new Control();
            _invoker.CreateControl();
        }

        #endregion
    }//end class 
}
