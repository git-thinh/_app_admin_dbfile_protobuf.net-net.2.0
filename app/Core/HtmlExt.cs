using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using mshtml;
using System.Net;
using System.Windows.Forms;
using app.GUI;
using app.Core;
using app.Model;
using System.Web;

namespace System
{
    public static class HtmlExtract_Ext
    {
        public static string getDomainFromURL(this string url)
        {
            string[] a = url.Split('/');
            string dom = a[2];
            a = dom.Split('.');
            if (a.Length == 1)
                dom = a[0];
            else if (a[a.Length - 2].Length > 3) dom = a[a.Length - 2] + "." + a[a.Length - 1];
            return dom;
        }

        public static string getContentTextFromURL(this string url)
        {
            WebClient objWebClient = new WebClient();
            url = HttpUtility.UrlDecode(url);
            byte[] buf = objWebClient.DownloadData(url);
            string htm = Encoding.UTF8.GetString(buf);
            //string htm = File.ReadAllText("demo1.html");
            string text = "";

            #region [ === FORMAT TAG HTML === ]

            htm = Regex.Replace(htm, @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<head[^>]*>[\s\S]*?</head>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<style[^>]*>[\s\S]*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            Regex regx = new Regex(@"<body[^>]*>([\s\S]*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match match = regx.Match(htm);
            if (match.Success)
                htm = match.Groups[1].Value;

            //htm = Regex.Replace(htm, @"<iframe[^>]*>[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
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

            var rx = new Regex(@"<iframe.+?src=[""'](.+?)[""'].*?>[\s\S]*?</iframe>");
            foreach (Match mi in rx.Matches(htm))
            {
                string tag = mi.ToString(), src = mi.Groups[1].Value;
                if (!string.IsNullOrEmpty(src) && src.ToLower().Contains("youtube.com"))
                {
                    string[] ai = src.Split('/');
                    string id_ = new string(ai[ai.Length - 1].ToCharArray().Where(c => char.IsLetterOrDigit(c)).ToArray());
                    htm = htm.Replace(tag, Environment.NewLine + "{ytu " + id_ + " ytu}" + Environment.NewLine);
                }
                else
                    htm = htm.Replace(tag, "");
            }

            #endregion

            #region [ === GET CONTENT BY IE === ]

            ////// Loading HTML Page into DOM Object
            ////IHTMLDocument2 doc = new HTMLDocumentClass();
            ////doc.clear();
            ////doc.write(htm);
            ////doc.close();

            //////' The following is a must do, to make sure that the data is fully load.
            ////while (doc.readyState != "complete")
            ////{
            ////    //This is also a important part, without this DoEvents() appz hangs on to the “loading”
            ////    //System.Windows.Forms.Application.DoEvents(); 
            ////    ;
            ////}

            //////string source = doc.body.outerHTML; 
            ////text = doc.body.innerText;

            #endregion

            text = new HtmlToText().ConvertContentTo(htm);

            ////List<string> listImg = new List<string>();
            ////var ps2 = Regex.Matches(text, "{img(.+?)img}", RegexOptions.IgnoreCase);
            ////foreach (Match mi in ps2)
            ////{
            ////    string img = mi.ToString();
            ////    listImg.Add(img);
            ////}

            return text;
        }


        public static int getIndex(this IndexLine[] a, ConfigItem _config, int indexMax)
        {
            string config = _config.Text;
            int index = -1;
            if (config.StartsWith("___"))
            {
                config = config.Substring(3, config.Length - 3);
                var ai = a.Where((x, k) => x.Line.EndsWith(config)).ToArray();
                if (indexMax > 0)
                    ai = ai.Where(x => x.Index <= indexMax).ToArray();
                if (ai.Length > 0)
                    index = ai[ai.Length - 1].Index;
            }
            else if (config.EndsWith("___"))
            {
                config = config.Substring(0, config.Length - 3);
                var ai = a.Where((x, k) => x.Line.StartsWith(config)).ToArray();
                if (indexMax > 0)
                    ai = ai.Where(x => x.Index <= indexMax).ToArray();
                if (ai.Length > 0)
                    index = ai[ai.Length - 1].Index;
            }
            else
            {
                config = config.Trim();
                var ai = a.Where((x, k) => x.Line.Equals(config)).ToArray();
                if (indexMax > 0)
                    ai = ai.Where(x => x.Index <= indexMax).ToArray();
                if (ai.Length > 0)
                    index = ai[ai.Length - 1].Index;
            }
            if (indexMax > 0) // first
                index += _config.SkipLine;
            else // last
                index -= _config.SkipLine;

            return index;
        }


        public static CmsExtract get_CmsExtract(this string content, string url, CNSPLIT config, string htm)
        {
            CmsExtract cms = new CmsExtract();
            cms.PLAIN_TEXT = content;

            string[] a = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Where(x => x != "").ToArray();
            if (config.SKIP_LINE_TOP == 1)
            {
                cms.Title = a[0];
                cms.Description = a[1];
                a = a.Where((x, k) => k > 1).ToArray();
            }
            else
            {
                cms.Description = a[0];
                a = a.Where((x, k) => k > 0).ToArray();
            }

            if (config.SKIP_LINE_BOTTOM == 1)
                a = a.Where((x, k) => k != a.Length - 1).ToArray();

            string[] ah = a.Select(x => string.Format("<div class={0}>{1}</div>", (x.Contains("{img") ? "_DIMG" : "_DTEXT"), x)).ToArray();
            string _htmContent = string.Join(Environment.NewLine + Environment.NewLine, ah);

            List<string> listImg = new List<string>();
            var ps2 = Regex.Matches(_htmContent, "{img(.+?)img}", RegexOptions.IgnoreCase);
            foreach (Match mi in ps2)
            {
                string img = mi.ToString(), src = img.Substring(4, img.Length - 8);
                string tag = string.Format(Environment.NewLine + "<p class=_PIMG><img class=\"_IMG\" src=\"{0}\"></p>" + Environment.NewLine, src);
                _htmContent = _htmContent.Replace(img, tag);
                listImg.Add(src);
            }

            cms.IMG_SRC = listImg.ToArray();
            if (listImg.Count > 0) cms.ImgDefault = listImg[0];
            cms.ContentHtml = _htmContent;

            return cms;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////// 

    public class HtmlToText
    {
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

        public string ConvertToTextPlain(string html)
        {
            html = Regex.Replace(html, "</td>", " {|t} </td>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = html.Replace(">", "> ");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString().Replace("{|t}", "\t");
        }

        public string ConvertHtml(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public string ConvertContentTo(string html)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertContentTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString().Trim();
        }

        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
                ConvertTo(subnode, outText);
        }

        public void ConvertTo(HtmlNode node, TextWriter outText)
        {

            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    string htm = node.OuterHtml.ToString();
                    var oh = node.Attributes.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    if (oh.Where(x =>
                        x.Contains("facebook") || x.Contains("twitter")
                    ).Count() > 0) return;

                    switch (node.Name)
                    {
                        case "p":
                        case "div":
                        case "br":
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            // treat paragraphs as crlf
                            // outText.Write("\r\n");
                            outText.Write(Environment.NewLine);
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }
    }


    /*

        The new Google spreadsheets use a slightly different URL:
        https://docs.google.com/spreadsheets/d/KEY/export?format=csv&id=KEY&gid=0

        No idea why the key appears twice in the URL.
        @hhblaze
        hhblaze commented on May 20, 2016
        Instead of

        var outputCSVdata = wc.DownloadString(url);
        better to use

        byte[] dt = wc.DownloadData(url)
        var outputCSVdata = System.Text.Encoding.UTF8.GetString(dt ?? new byte[] {})
        to have proper csv Encoding

     1. Your Google SpreadSheet document must be set to 'Anyone with the link' can view it

     2. To get URL press SHARE (top right corner) on Google SpreeadSheet and copy "Link to share".

     3. Now add "&output=csv" parameter to this link

     4. Your link will look like:

        https://docs.google.com/spreadsheet/ccc?key=1234abcd1234abcd1234abcd1234abcd1234abcd1234&usp=sharing&output=csv

        */
    //string url = @"https://docs.google.com/spreadsheet/ccc?key=1234abcd1234abcd1234abcd1234abcd1234abcd1234&usp=sharing&output=csv"; // REPLACE THIS WITH YOUR URL
    //url = @"https://docs.google.com/spreadsheet/ccc?key=131_R7NGjGdkusmJzzp46BrGGf6Wk9XO-L0j0Jmi-5Yg&usp=sharing&output=csv"; // REPLACE THIS WITH YOUR URL
    //url = "https://drive.google.com/file/d/0B7ME5un_RN3xbExneUk4TVZRQzA/view";
    //url = "https://docs.google.com/document/export?format=txt&id=1cNbF79etCUrSbaNatdViPQiv2VIuOoNVZoaaDyvKPDY";

    //WebClientEx wc = new WebClientEx(new CookieContainer());
    //wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
    //wc.Headers.Add("DNT", "1");
    //wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    //wc.Headers.Add("Accept-Encoding", "deflate");
    //wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");

    ////var outputCSVdata = wc.DownloadString(url);
    //byte[] buf = wc.DownloadData(url);
    //string s = Encoding.UTF8.GetString(buf);


    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        private readonly CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}
