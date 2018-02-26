using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CassiniDev
{
    class Program
    {
        static void Main(string[] paras)
        {
            int port = 12345;
            string path = @"C:\";

            if (paras.Length == 0)
            {
                #region [ === RUN VIA CONSOLE === ]

                path = //@"D:\WebUI\webui\dev\source\study\www";// Environment.CurrentDirectory;
                path = Environment.CurrentDirectory;

                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                
                var server = new Server(port, "/", path, false, false);
                // HostsFile.AddHostEntry(server.IPAddress.ToString(), server.HostName);

                try
                {
                    server.Start();
                    Console.Title = port.ToString();
                    string url = string.Format("http://localhost:{0}/index.html", port);
                    string chrome = @"D:\ChromeDoc\GoogleChromePortable.exe";
                    if (!File.Exists(chrome)) chrome = "chrome.exe";
                    Process.Start(chrome, url);
                    Console.WriteLine(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]");
                    //Console.WriteLine("ERROR: " + ex.Message);
                }

                Console.ReadLine();

                #endregion
            }
            else
            {
                #region [  === RUN VIA PROCESS === ]

                if (paras.Length > 1)
                {
                    int.TryParse(paras[0], out port);
                    path = paras[1];
                }

                string pr = string.Join("||", paras);
                Console.WriteLine(pr);

                CommandLineArguments args = new CommandLineArguments()
                {
                    Port = port,
                    VirtualPath = "/",
                    ApplicationPath = path,
                    Ntlm = false,
                    Nodirlist = false,
                };

                var server = new Server(args.Port, args.VirtualPath, args.ApplicationPath, args.Ntlm, args.Nodirlist);

                if (args.AddHost)
                {
                    HostsFile.AddHostEntry(server.IPAddress.ToString(), server.HostName);
                }

                try
                {
                    server.Start();
                    Console.WriteLine("[BEGIN]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]");
                    //Console.WriteLine("ERROR: " + ex.Message);
                }

                //Console.ReadLine();
                while (true) {; }

                #endregion
            }

        }
    }
}
