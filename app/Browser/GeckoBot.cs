namespace app
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Gecko;

    public static class GeckoBot
    {
        public static string url_Current = "http://localhost:61422/";

        public static void RUN()
        {
            string path = Path.Combine(Application.StartupPath, @"Service\FF33");
            Init(path);
            var browser = CreateBrowser();



            browser.Invoke(new Action(() =>
            {
                browser.Navigate(url_Current);

                browser.DocumentTitleChanged += (se2, ev2) => {
                    string url = browser.Url.ToString();
                    MessageBox.Show(url);
                };
                browser.DocumentCompleted += (se2, ev2) =>
                {
                    string JS_DOM = @"setTimeout(function(){ document.getElementById('submit_approve_access').click(); },3000); ";
                    using (AutoJSContext context = new AutoJSContext(browser.Window.JSContext))
                    {
                        string rs;
                        context.EvaluateScript(JS_DOM, (nsISupports)browser.Window.DomWindow, out rs);
                    }
                };
            }));

        }

        #region [ BOT BROWSER ]

        /// <summary>
        /// create browser
        /// </summary>
        /// <returns></returns>
        private static GeckoWebBrowser CreateBrowserInternal()
        {
            GeckoWebBrowser browser = new GeckoWebBrowser();
            browser.CreateControl();
            browser.AddMessageEventListener("__post", ((string data) =>
            {
                data = System.Web.HttpUtility.UrlDecode(data);

            }));
            //////browser.NavigateFinishedNotifier.BlockUntilNavigationFinished();
            ////browser.JavascriptError += (sender, error) =>
            ////{
            ////    // do something
            ////};
            return browser;
        }

        private static volatile bool _initialized = false;
        private static Thread _appThread;
        private static Control _invoker; // very simple method to create new browsers is to use Invoke of this control
        private static string _xulPath;

        /// <summary>
        /// Create browser. Call it to get new instance of Gecko browser
        /// </summary>
        /// <returns></returns>
        public static GeckoWebBrowser CreateBrowser()
        {
            ////if (Xpcom.IsInitialized == false && _initialized == false)
            ////{
            ////    Thread.Sleep(2000);
            ////}
            ////_initialized = true;
            while (_initialized == false) { Thread.Sleep(500); }

            //if (!_initialized) throw new InvalidOperationException("xul is not initialized yet");
            //if (!Xpcom.IsInitialized) throw new InvalidOperationException("xul is not initialized yet");
            return (GeckoWebBrowser)_invoker.Invoke(new Func<GeckoWebBrowser>(CreateBrowserInternal));
        }

        /// <summary>
        /// init xul and start message loop
        /// </summary>
        private static void InitXul()
        {
            _invoker = new Control();
            _invoker.CreateControl();

            Xpcom.AfterInitalization += () =>
            {
                _initialized = true;
            };

            Xpcom.Initialize(_xulPath);

            //while (_initialized == false) { Thread.Sleep(500); }

            //////using lots of computer memory.
            ////var _memoryService = Xpcom.GetService<nsIMemory>("@mozilla.org/xpcom/memory-service;1");
            ////_memoryService.HeapMinimize(true);

            //GeckoPreferences.User["security.warn_viewing_mixed"] = false;
            GeckoPreferences.User["plugin.state.flash"] = 0;
            GeckoPreferences.User["browser.cache.disk.enable"] = false;
            GeckoPreferences.User["browser.cache.memory.enable"] = false;
            GeckoPreferences.User["permissions.default.image"] = 2;

            GeckoPreferences.User["browser.xul.error_pages.enabled"] = false;
            GeckoPreferences.User["security.enable_ssl2"] = true;
            GeckoPreferences.User["security.default_personal_cert"] = "Ask Never";
            GeckoPreferences.User["security.warn_entering_weak"] = false;
            GeckoPreferences.User["security.warn_viewing_mixed"] = false;
            GeckoPreferences.User["dom.disable_open_during_load"] = true;
            GeckoPreferences.User["dom.allow_scripts_to_close_windows"] = false;
            GeckoPreferences.User["dom.popup_maximum"] = 0;
            ////GeckoPreferences.User["dom.max_script_run_time"] = 5;

            PromptFactory.PromptServiceCreator = () => new FilteredPromptService();

            ////////foreach (Gecko.Plugins.PluginTag tag in Gecko.Plugins.PluginHost.GetPluginTags())
            ////////{
            ////////    // if (tag.Name.Contains("Flash"))
            ////////    tag.Disabled = true;
            ////////}

            Application.Run();
        }


        /// <summary>
        /// is xul initialized
        /// </summary>
        public static bool IsInitialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// Initialize xul
        /// </summary>
        /// <param name="xulrunnerPath">путь к движку</param>
        public static void Init(string xulrunnerPath)
        {
            if (_initialized)
                return;
            if (!Directory.Exists(xulrunnerPath))
                throw new DirectoryNotFoundException(xulrunnerPath);
            _xulPath = xulrunnerPath;
            _appThread = new Thread(InitXul);
            _appThread.SetApartmentState(ApartmentState.MTA);
            _appThread.Start();

        }


        #endregion
    }

}