using System.Drawing;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.IO;
using System;
using mshtml; 
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Text;

namespace app.GUI.Html
{
    class oTagSplit
    {
        public string Domain { set; get; }
        public string TrimLeft { set; get; }
        public string TrimRight { set; get; }
        public int TrimRowLeft { set; get; }
        public int TrimRowRight { set; get; }
    }
     
    public class WebButton : IHTMLEditorButton
    {
        #region [ === === ]
         
        static string getContent(string url, string text)
        {
            string[] a = url.Split('/');
            string dom = a[2];
            a = dom.Split('.');
            if (a[a.Length - 2].Length > 3) dom = a[a.Length - 2] + "." + a[a.Length - 1];

            var adom = listTagSplit.Where(x => x.Domain == dom).ToArray();
            if (adom.Length > 0)
            {
                oTagSplit rs = adom[0];

                if (rs != null && !string.IsNullOrEmpty(rs.TrimLeft))
                {
                    a = text.Split(new string[] { br }, StringSplitOptions.None)
                        .Select(x => x.Trim()).Where(x => x != "").ToArray();
                    text = string.Join(br, a);

                    int pcut = text.IndexOf(rs.TrimLeft);
                    if (pcut > 0)
                        text = text.Substring(pcut + rs.TrimLeft.Length);

                    pcut = text.IndexOf(rs.TrimRight);
                    if (pcut > 0)
                        text = text.Substring(0, pcut);

                    var ls = text.Split(new string[] { br }, StringSplitOptions.None)
                        .Select(x => x.Trim()).Where(x => x != "").ToList();
                    if (rs.TrimRowLeft > 0)
                        for (int k = 0; k < rs.TrimRowLeft; k++)
                            ls.RemoveAt(0);
                    if (rs.TrimRowRight > 0)
                        for (int k = 0; k < rs.TrimRowRight; k++)
                            ls.RemoveAt(ls.Count - 1);
                    text = string.Join(br, ls.ToArray());
                }
            }
            return text.Trim();
        }

        static string br = Environment.NewLine;
        static List<oTagSplit> listTagSplit = new List<oTagSplit>()
        {
            new oTagSplit(){ Domain = "vnexpress.net", TrimLeft = "GMT+7" + br, TrimRight = br + "Xem thêm:" + br, TrimRowLeft = 2, TrimRowRight = 1 },
            new oTagSplit(){ Domain = "zing.vn", TrimLeft = br + "Nhịp sống" + br, TrimRight = "Bình luận" + br, TrimRowLeft = 0, TrimRowRight = 2 }, 
        };

        private string Crawler(string url)
        {
            WebClient objWebClient = new WebClient();
            byte[] buf = objWebClient.DownloadData(url);
            string htm = Encoding.UTF8.GetString(buf);
            //string htm = File.ReadAllText("demo2.html");


            htm = Regex.Replace(htm, @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<head[^>]*>[\s\S]*?</head>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<style[^>]*>[\s\S]*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<iframe[^>]*>[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<form[^>]*>[\s\S]*?</form>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            htm = Regex.Replace(htm, @"<ul[^>]*>[\s\S]*?</ul>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var ps = Regex.Matches(htm, "<img.+?src=[\"'](.+?)[\"'].*?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            foreach (Match mi in ps)
            {
                string img = mi.ToString(), src = mi.Groups[1].Value;
                //string id = " " + Guid.NewGuid().ToString() + " ";
                string id = " {img" + src + "img} ";

                int p = htm.IndexOf(img);
                if (p > 0)
                {
                    string s0 = htm.Substring(0, p),
                        s1 = htm.Substring(p + img.Length, htm.Length - (p + img.Length));
                    int pend = s1.IndexOf(">");
                    if (pend != -1) s1 = s1.Substring(pend + 1);
                    htm = s0 + id + s1;
                }
                //htm = htm.Replace(img, id); 
            }


            // Loading HTML Page into DOM Object
            IHTMLDocument2 doc = new HTMLDocumentClass();
            doc.clear();
            doc.write(htm);
            doc.close();


            //////var htmlDoc = new HTMLDocumentClass();
            //////var ips = (IPersistStreamInit)htmlDoc;
            //////ips.InitNew(); 
            //////var doc = htmlDoc.createDocumentFromUrl(url, "null"); 
            //' The following is a must do, to make sure that the data is fully load.
            while (doc.readyState != "complete")
            {
                //This is also a important part, without this DoEvents() appz hangs on to the “loading”
                //System.Windows.Forms.Application.DoEvents(); 
                ;
            }

            ////string js = "function(){ return 1; }";
            //////js = "functionName(param1);";
            ////js = "(function() { return confirm('Continue?'); })()";

            //doc.parentWindow.execScript(js, "javascript");

            //doc.parentWindow.alert("dsdsadsad");

            //HTMLWindow2 iHtmlWindow2 = (HTMLWindow2)doc.Script;
            //var rs = iHtmlWindow2.execScript(js, "javascript");

            //mshtml.IHTMLWindow2 win = doc.parentWindow as IHTMLWindow2;
            //win.execScript(js, "javascript");

            string source = doc.body.outerHTML; ;
            string text = doc.body.innerText;

            string content = getContent(url, text);

            List<string> listImg = new List<string>();
            var ps2 = Regex.Matches(text, "{img(.+?)img}", RegexOptions.IgnoreCase);
            foreach (Match mi in ps2)
            {
                string img = mi.ToString();
                listImg.Add(img);
            }

            ////var it = ((mshtml.HTMLDocument)doc).all.Cast<mshtml.IHTMLElement>().ToList();
            //var ls = doc.Where(x => x.Name == "q").ToList();
            //foreach (var li in ls) li.SetAttribute("value", "dây hàn nhôm");
            //doc.First(p => p.TagName == "FORM").InvokeMember("Submit");


            ////List<mshtml.IHTMLInputElement> allInput = doc.all.OfType<mshtml.IHTMLInputElement>().ToList();
            //((mshtml.IHTMLElement)doc.all.item("q")).setAttribute("value", "dây hàn nhôm");
            //((mshtml.IHTMLFormElement)doc.all.item("f")).submit();

            //Console.WriteLine(content);
            return content;
        }

        #endregion

        public void IconClicked(HTMLEditorButtonArgs args)
        {
            var x = args.Editor.Location.X + 10;
            var y = args.Editor.Location.Y + 10;
            
            string urc = Clipboard.GetText(TextDataFormat.Text);

            var url = Interaction.InputBox("Please enter an image url", "URL", urc, x, y);
            if (!string.IsNullOrEmpty(url))
            {
                string content = Crawler(url);
            } 

        } //end function

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("web.gif"); }
        }

        public string IconName
        {
            get { return "Copy content from page web"; }
        }

        public string IconTooltip
        {
            get { return "Copy content from page web"; }
        }

        public string CommandIdentifier
        {
            get { return "InsertImage"; }
        }
    }
}