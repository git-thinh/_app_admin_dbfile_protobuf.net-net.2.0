using Gecko;
using Gecko.Cache;
using Gecko.DOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace app.GUI
{
    public class FormGecko : Form
    {
        private string JS = "";
        private GStatus m_Status = GStatus.NONE;
        private string URL = "";
        private GeckoWebBrowser browser;

        public FormGecko()
        {
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            string path = Path.Combine(Application.StartupPath, @"Service\FF33");
            Xpcom.Initialize(path);

            login();
            //disable();
            //logout();

            #region [  === CONFIG GECKO === ]

            //////// Uncomment the follow line to enable error page
            //////GeckoPreferences.User["browser.xul.error_pages.enabled"] = true;
            //////GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
            //////GeckoPreferences.User["full-screen-api.enabled"] = true;
            //////GeckoPreferences.User["security.warn_viewing_mixed"] = false;
            //////GeckoPreferences.User["plugin.state.flash"] = 0;
            //////GeckoPreferences.User["browser.cache.disk.enable"] = false;
            //////GeckoPreferences.User["browser.cache.memory.enable"] = false;
            //////GeckoPreferences.User["permissions.default.image"] = 2;
            ////////string sUserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36";
            ////////GeckoPreferences.User["general.useragent.override"] = sUserAgent;


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

            #endregion

            browser = new GeckoWebBrowser();
            browser.Dock = DockStyle.Fill;
            this.Controls.Add(browser);
            browser.DocumentTitleChanged += (se2, ev2) => {
                string url = browser.Url.ToString();
                if (url.StartsWith("http://localhost:61422/?code="))
                    MessageBox.Show(url);
            };
            browser.DocumentCompleted += (se2, ev2) =>
            {
                string val = "";
                if (m_Status == GStatus.DISABLE) return;

                #region [ === LOGOUT === ]

                if (m_Status == GStatus.LOGOUT)
                { 
                    JS = @" var arr = [], l = document.links; for(var i=0; i<l.length; i++) { var _href = l[i].href; if(_href.indexOf('https://accounts.google.com/Logout?') == 0){ arr.push(_href); } } ; var _link = arr.join('\n'); _link; ";
                    using (AutoJSContext context = new AutoJSContext(browser.Window.JSContext))
                    {
                        context.EvaluateScript(JS, (nsISupports)browser.Window.DomWindow, out val);
                        if (!string.IsNullOrEmpty(val))
                        {
                            m_Status = GStatus.LOGOUT_FINISH;
                            browser.Navigate(val);
                        }
                    }

                    return;
                }

                if (m_Status == GStatus.LOGOUT_FINISH)
                {
                    clearCookie();
                    m_Status = GStatus.DISABLE;
                    browser.Navigate("https://www.google.com.vn/");
                    return;
                }

                #endregion

                #region [ === LOGIN === ]

                var submit_approve_access = browser.Document.GetElementById("submit_approve_access");
                if (submit_approve_access != null)
                    m_Status = GStatus.SUBMIT_APPROVE_ACCESS;

                switch (m_Status)
                {
                    case GStatus.NONE:
                    case GStatus.LOGIN_EMAIL:
                        var email = browser.Document.GetElementById("Email");
                        if (email != null)
                        {
                            m_Status = GStatus.LOGIN_EMAIL;

                            JS = @"setTimeout(function(){ document.getElementById('Email').value = 'phuquydoc@gmail.com'; document.getElementById('next').click(); },3000); ";
                            using (AutoJSContext context = new AutoJSContext(browser.Window.JSContext))
                            {
                                context.EvaluateScript(JS, (nsISupports)browser.Window.DomWindow, out val);
                                m_Status = GStatus.LOGIN_PASS;
                            }
                        }
                        break;
                    case GStatus.LOGIN_PASS:
                        var pass = browser.Document.GetElementById("password");
                        if (pass != null)
                        {
                            m_Status = GStatus.LOGIN_PASS;
                            JS = @"setTimeout(function(){ document.getElementById('password').value = 'thinhtu710'; document.getElementById('submit').click(); },3000); ";
                            using (AutoJSContext context = new AutoJSContext(browser.Window.JSContext))
                            {
                                context.EvaluateScript(JS, (nsISupports)browser.Window.DomWindow, out val);
                                m_Status = GStatus.SUBMIT_APPROVE_ACCESS;
                            }
                        }
                        break;
                    case GStatus.SUBMIT_APPROVE_ACCESS:
                        JS = @"setTimeout(function(){ document.getElementById('submit_approve_access').click(); },5000); ";
                        using (AutoJSContext context = new AutoJSContext(browser.Window.JSContext))
                        {
                            context.EvaluateScript(JS, (nsISupports)browser.Window.DomWindow, out val);
                            m_Status = GStatus.FINISH;
                        }
                        break;
                    case GStatus.FINISH:
                        break;
                }

                #endregion
            };

            this.Shown += (se, ev) =>
            {
                this.Hide();
                //this.WindowState = FormWindowState.Maximized;
                browser.Navigate(URL);
            };
        }//end C'tor

        void disable()
        {
            m_Status = GStatus.DISABLE;
            URL = "https://www.google.com.vn/";
        }

        void logout()
        {
            m_Status = GStatus.LOGOUT;
            URL = "https://www.google.com.vn/";
        }

        void login()
        {
            clearCookie();
            m_Status = GStatus.NONE;
            URL = "http://localhost:61422/";
        }

        public void Close()
        {
            browser.Dispose();
        }

        void clearCookie()
        {
            nsICookieManager CookieMan;
            CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
            CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
            CookieMan.RemoveAll();

            ////nsIBrowserHistory historyMan = Xpcom.GetService<nsIBrowserHistory>(Gecko.Contracts.NavHistoryService);
            ////historyMan = Xpcom.QueryInterface<nsIBrowserHistory>(historyMan);
            ////historyMan.RemoveAllPages();

            //// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/imgICache
            //ImageCache.ClearCache(true);
            //ImageCache.ClearCache(false);
            //// Defaults to all devices(0) - https://bitbucket.org/geckofx/geckofx-9.0/issue/7/idl-translation-bug-for-enums
            //CacheService.Clear(new CacheStoragePolicy());
        }

        public enum GStatus
        {
            DISABLE,
            LOGOUT,
            LOGOUT_FINISH,
            NONE,
            LOGIN_EMAIL,
            LOGIN_PASS,
            SUBMIT_APPROVE_ACCESS,
            FINISH
        }


    }// end class
}
