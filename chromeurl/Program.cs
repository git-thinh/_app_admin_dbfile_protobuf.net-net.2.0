using System;
using System.Collections.Generic; 
using System.Text;
using System.Windows.Automation;
using System.Diagnostics; 

namespace chromeurl
{
    class Program
    {
        static void Main(string[] args)
        {
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                // the chrome process must have a window
                if (chrome.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }

                // find the automation element
                AutomationElement elm = AutomationElement.FromHandle(chrome.MainWindowHandle);
                AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants,
                  new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));

                // if it can be found, get the value from the URL bar
                if (elmUrlBar != null)
                {
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                    if (patterns.Length > 0)
                    {
                        ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
                        Console.WriteLine(val.Current.Value);
                        break;
                        //listbox.Items.Add(val.Current.Value);
                    }
                }
            }

            //AutomationElement.RootElement
            //.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"))
            //.SetFocus();
            //System.Windows.Forms.SendKeys.SendWait("^l");
            //var elmUrlBar = AutomationElement.FocusedElement;
            //var valuePattern = (ValuePattern)elmUrlBar.GetCurrentPattern(ValuePattern.Pattern);
            //string url = valuePattern.Current.Value;
            //Console.WriteLine(url);
        }
    }
}
