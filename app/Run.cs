using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace app
{
    class Run
    {
        //[STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] ar)
        {
            App.Start();
        }


    }
}
