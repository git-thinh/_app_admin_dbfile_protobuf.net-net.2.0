namespace app
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using Gecko;

    public static class GeckoHeadless
    {

        public static void Test()
        {
            GeckoHeadless.Init("f22");

            var browser = GeckoHeadless.CreateBrowser();
            browser.Invoke(new Action(() =>
            {


                //browser.Navigate("http://localhost:8080/");
                browser.Navigate("https://google.com.vn/");
                browser.Navigated += (sender, args) =>
                {
                    var text = browser.Window.Document.TextContent; // text is null
                };
                browser.DocumentCompleted += (sender, even) =>
                {
                    //GeckoElement head = browser.Window.Document.GetElementsByTagName("head")[0];
                    //GeckoElement scriptEl = browser.Window.Document.CreateElement("script");
                    //string alertBlocker = "window.alert = function () { }";
                    //scriptEl.InnerHtml = alertBlocker;
                    //head.AppendChild(scriptEl);

                    var text = browser.Window.Document.TextContent; // text is null

                    //browser.Navigate("javascript:alert('" + Guid.NewGuid().ToString() + "');");
                    browser.Navigate("javascript:alert('" + Guid.NewGuid().ToString() + "');");
                    Thread.Sleep(3000);
                    browser.Navigate("javascript:alert('" + Guid.NewGuid().ToString() + "');");
                    //browser.Navigate("javascript:setInterval(function () { send('" + Guid.NewGuid().ToString() + "'); }, 3000);");


                };

                Console.WriteLine("end browser ...");
            }));
        }


        private static volatile bool _initialized = false;
        private static Thread _appThread;
        private static Control _invoker; // very simple method to create new browsers is to use Invoke of this control
        private static string _xulPath;

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

            //////using lots of computer memory.
            ////var _memoryService = Xpcom.GetService<nsIMemory>("@mozilla.org/xpcom/memory-service;1");
            ////_memoryService.HeapMinimize(true);
             
            Application.Run();
        }

        /// <summary>
        /// create browser
        /// </summary>
        /// <returns></returns>
        private static GeckoWebBrowser CreateBrowserInternal()
        {
            GeckoWebBrowser browser = new GeckoWebBrowser();
            browser.CreateControl();
            return browser;
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


    }
}