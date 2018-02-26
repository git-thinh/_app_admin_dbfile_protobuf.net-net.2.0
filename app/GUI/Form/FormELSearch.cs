using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using mshtml;
using System.Linq;
using System.Linq.Dynamic;
using System.IO;
using System.Data;
using HtmlAgilityPack;
using app.Core;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using app.Model;

namespace app.GUI
{
    public class FormELSearch : Form
    {
        #region [ === VARIABLE === ]

        const int _form_Width = 300;
        const int _form_Height = 200;
        const int _resize_Width = 5;
        const int _header_Height = 18;

        private readonly Padding _form_Padding = new Padding(1);
        private readonly Color _form_BorderColor = Color.Black;
        private readonly Color _form_BgColor = Color.WhiteSmoke;
        private readonly Color _header_BgColor = Color.Yellow;
        private readonly Color _main_BgColor = Color.Orange;

        private readonly TextBoxCustom txtSearch;
        private readonly Panel boxMain;
        private readonly Panel boxFooter;

        private readonly RadioButton radDOC;
        private readonly RadioButton radWORD;
        private readonly RadioButton radGRAM;
        private readonly Button btnChrome;

        private Process process;
        private readonly IDataFile db;
        #endregion

        public FormELSearch(IDataFile _db)
        {
            db = _db;
            Control.CheckForIllegalCrossThreadCalls = false;
            ShowInTaskbar = false;

            btnChrome = new Button()
            {
                Text = "chrome",
                Top = 0,
                Left = 160,
                Width = 55,
                Height = 19,
                ForeColor = Color.Black,
                BackColor = SystemColors.Control,

            };
            /////////////////////////////////////////////////////
            // CUSTOM RESIZE HANDLE IN BORDER-LESS FORM
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            /////////////////////////////////////////////////////
            // FORM PROPERTIES
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(_form_Width, _form_Height);
            this.Padding = _form_Padding;
            this.BackColor = App.ColorBg;

            /////////////////////////////////////////////////////
            // UI: MENU CONTEXT
            ContextMenu menuMain = new ContextMenu(build_menuItem());

            /////////////////////////////////////////////////////
            // UI: HEADER SEARCH
            Panel boxHeader = new Panel() { Left = 1, Top = 1, BackColor = _header_BgColor, Height = _header_Height };
            txtSearch = new TextBoxCustom()
            {
                Left = 7,
                Top = 2,
                Height = _header_Height - 2,
                Multiline = true,
                ForeColor = Color.OrangeRed,
                BackColor = _header_BgColor,
                WaterMark = "Search ... ",
                BorderStyle = BorderStyle.None,
            };
            boxHeader.Controls.AddRange(new Control[] { txtSearch });

            /////////////////////////////////////////////////////
            // UI: PANEL MAIN
            boxMain = new Panel()
            {
                Left = 1,
                Top = _header_Height + 2,
                Width = _form_Width - 2,
                Height = _form_Height - (_header_Height + 17 + 2 + 3),
                BackColor = _main_BgColor
            };
            boxMain.MouseDown += FormMove_MouseDown;
            boxMain.ContextMenu = menuMain;
            boxMain.AllowDrop = true;
            boxMain.DragEnter += new DragEventHandler(boxMain_DragEnter);
            boxMain.DragDrop += new DragEventHandler(boxMain_DragDrop);
            /////////////////////////////////////////////////////
            // FOOTER
            boxFooter = new Panel() { Dock = DockStyle.Bottom, Height = 17 };
            boxFooter.MouseMove += FormMove_MouseDown;
            radWORD = new RadioButton() { Text = "Word", Top = 0, Left = 10, Width = 50, Height = 17, Checked = true, ForeColor = Color.White };
            radGRAM = new RadioButton() { Text = "Gram", Top = 0, Left = 62, Width = 50, Height = 17, ForeColor = Color.White };
            radDOC = new RadioButton() { Text = "Doc", Top = 0, Left = 114, Width = 44, Height = 17, ForeColor = Color.White };
            btnChrome.Click += (se, ev) => chrome_Extract();
            boxFooter.Controls.AddRange(new Control[] {
                radWORD,
                radGRAM,
                radDOC ,
                btnChrome,
            });

            /////////////////////////////////////////////////////
            // EVENT
            this.Controls.AddRange(new Control[] {
                boxHeader,
                boxMain,
                boxFooter,
            });

            this.Shown += (se, ve) =>
            {
                this.Left = 0;
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - _form_Height;

                boxHeader.Width = _form_Width - _resize_Width;
                boxHeader.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                txtSearch.Width = _form_Width - txtSearch.Left;
                txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
                boxMain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            };
            txtSearch.KeyPress += (se, ev) =>
            {
            };
        }

        MenuItem[] build_menuItem()
        {
            MenuItem[] mi = new MenuItem[3];
            mi[0] = new MenuItem("Add word at header", menuItem_Click);
            mi[1] = new MenuItem("Item2", menuItem_Click);
            mi[2] = new MenuItem("Item3", menuItem_Click);
            return mi;
        }

        void chrome_Extract()
        {
            Cursor.Current = Cursors.WaitCursor;

            string fi = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Service\chromeurl.exe");
            process = new Process();
            process.StartInfo.FileName = fi;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.OutputDataReceived += (se, ev) =>
            {
                string url = ev.Data;
                if (!string.IsNullOrEmpty(url))
                {
                    //MessageBox.Show(url);
                    url_Extract("http://" + url);
                    Cursor.Current = Cursors.Default;
                }
            };

            //* Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();

            Cursor.Current = Cursors.Default;
        }

        void url_buildConfig(string url, string content_ = "")
        {
            var fc = new FormDataSplitConfig(db, url, content_);
            fc.OnSubmit += (config, htm, content) =>
            {
                CmsExtract cms = content.get_CmsExtract(url, config, htm);
                fc.Close();
            };
            fc.ShowDialog();
        }

        void url_Extract(string url)
        {
            string dom = url.getDomainFromURL();
            SearchResult rs = db.FindItemByContainFieldValue(new CNSPLIT() { SITE = dom }, "SITE", 1000, 1);
            if (rs == null || rs.Total == 0)
                url_buildConfig(url);
            else
            {
                string Content = url.getContentTextFromURL();
                var ls = ((IList)rs.Message).Convert<CNSPLIT>().ToArray();

                CNSPLIT config = (CNSPLIT)ls[0];
                IndexLine[] a = Content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 1).Select((x, k) => new IndexLine(k, x)).ToArray();
                int pLast = a.getIndex(new ConfigItem() { Text = config.TEXT_LAST, SkipLine = config.SKIP_LINE_BOTTOM }, -1), 
                    pFirst = a.getIndex(new ConfigItem() { Text = config.TEXT_FIRST, SkipLine = config.SKIP_LINE_TOP }, pLast);
                if (pFirst == -1 || pLast == -1)
                {
                    //MessageBox.Show("Config wrong, can not find position first or last of data content");
                    url_buildConfig(url, Content);
                    return;
                }
                string[] con = a.Where(x => x.Index > pFirst && x.Index < pLast).Select(x => x.Line).ToArray();
                string data = string.Join(Environment.NewLine, con);

                //CmsExtract cms = data.get_CmsExtract(url, config, "");
                new Thread(new ParameterizedThreadStart((x) =>
                {
                    new FormELDocAdd(url, x.ToString(), new string[] { }).ShowDialog();
                })).Start(data);
            }
        }

        void boxMain_DragEnter(object sender, DragEventArgs e)
        {
            object dt = e.Data;
            //if (e.Data.GetDataPresent(DataFormats.UnicodeText)) e.Effect = DragDropEffects.Copy;
            if (e.Data.GetDataPresent("text/html")) e.Effect = DragDropEffects.Copy;
        }

        void boxMain_DragDrop(object sender, DragEventArgs e)
        {
            string htm = null;

            #region [ === GET DATA FROM DROP DRAP === ]

            //use the underlying IDataObject to get the FileGroupDescriptorW as a MemoryStream
            MemoryStream dataFormatStream = (MemoryStream)e.Data.GetData("text/html");
            byte[] dataFormatBytes = new byte[dataFormatStream.Length];
            dataFormatStream.Read(dataFormatBytes, 0, dataFormatBytes.Length);
            dataFormatStream.Close();

            //if (dataFormatBytes[1] == 0)
            //    htm = Encoding.Unicode.GetString(dataFormatBytes);
            //else
            //    htm = Encoding.ASCII.GetString(dataFormatBytes); 
            htm = Encoding.UTF8.GetString(dataFormatBytes);

            #endregion


            List<string> listMp3 = new List<string>();
            var ps2 = Regex.Matches(htm, "http(.+?)mp3", RegexOptions.IgnoreCase);
            foreach (Match mi in ps2)
            {
                string img = mi.ToString();
                listMp3.Add(img);
            }

            if (radWORD.Checked)
            {
                #region
                List<string[]> li = new List<string[]>() { };
                if (htm.Contains("<table"))
                    li = DataExtract.format_TableHTML(htm);
                else
                    li = DataExtract.format_TextHtml(htm);
                new FormELWordAdd(li, listMp3.ToArray()).Show();
                #endregion
            }
            else if (radGRAM.Checked)
            {
                string text = DataExtract.format_TextPlain(htm);
                new FormELGramAdd(text, listMp3.ToArray()).Show();
            }
            else
            {
                #region
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
                string text = DataExtract.format_TextPlain(htm);

                //List<string> listImg = new List<string>();
                //var ps2 = Regex.Matches(text, "{img(.+?)img}", RegexOptions.IgnoreCase);
                //foreach (Match mi in ps2)
                //{
                //    string img = mi.ToString();
                //    listImg.Add(img);
                //}
                new FormELDocAdd("", text, listMp3.ToArray()).Show();
                #endregion
            }
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                mi.Checked = true;
                MessageBox.Show(mi.Text);
            }
        }

        #region [ === CUSTOM RESIZE HANDLE IN BORDER-LESS FORM === ]

        const uint WM_NCHITTEST = 0x0084, WM_MOUSEMOVE = 0x0200,
                HTLEFT = 10, HTRIGHT = 11, HTBOTTOMRIGHT = 17,
                HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTTOP = 12,
                HTTOPLEFT = 13, HTTOPRIGHT = 14;
        Size formSize;
        Point screenPoint;
        Point clientPoint;
        Dictionary<uint, Rectangle> boxes;
        const int RHS = 10; // RESIZE_HANDLE_SIZE
        bool handled;

        protected override void WndProc(ref Message m)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                return;
            }

            handled = false;
            if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE)
            {
                formSize = this.Size;
                screenPoint = new Point(m.LParam.ToInt32());
                clientPoint = this.PointToClient(screenPoint);

                boxes = new Dictionary<uint, Rectangle>() {
                {HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RHS, RHS, RHS)},
                {HTBOTTOM, new Rectangle(RHS, formSize.Height - RHS, formSize.Width - 2*RHS, RHS)},
                {HTBOTTOMRIGHT, new Rectangle(formSize.Width - RHS, formSize.Height - RHS, RHS, RHS)},
                {HTRIGHT, new Rectangle(formSize.Width - RHS, RHS, RHS, formSize.Height - 2*RHS)},
                {HTTOPRIGHT, new Rectangle(formSize.Width - RHS, 0, RHS, RHS) },
                {HTTOP, new Rectangle(RHS, 0, formSize.Width - 2*RHS, RHS) },
                {HTTOPLEFT, new Rectangle(0, 0, RHS, RHS) },
                {HTLEFT, new Rectangle(0, RHS, RHS, formSize.Height - 2*RHS) }
            };

                foreach (var hitBox in boxes)
                {
                    if (hitBox.Value.Contains(clientPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
                base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                ControlPaint.DrawSizeGrip(e.Graphics, Color.Red,
                    //this.ClientSize.Width - 16, this.ClientSize.Height - 16, 16, 16);
                    this.ClientSize.Width - 16, 0, 16, 16);
            }

            base.OnPaint(e);
        }

        #endregion

        #region [ === MOVE FROM === ]

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public void FormMove_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

    }

    public class FormELDocAdd : Form
    {
        private readonly Panel boxWord;
        private readonly TextBox txtContent;
        private readonly FlowLayoutPanel boxFooter;
        private readonly Button btnSubmit;
        public FormELDocAdd(string url, string text, string[] urlResourceMp3)
        {
            this.Font = App.Font;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(800, 500);

            string[] a = text.Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            string _title = a[0];
            string _content = string.Join(Environment.NewLine, a.Where((x, k) => k > 0).ToArray()).Trim();

            boxWord = new Panel() { Dock = DockStyle.Top, Height = 22, Padding = new Padding(10, 3, 0, 0), BackColor = Color.LightPink };
            boxWord.Controls.Add(new TextBox() { Text = _title, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Color.LightPink });

            txtContent = new TextBox() { Dock = DockStyle.Fill, Multiline = true, WordWrap = true, ScrollBars = ScrollBars.Vertical, Text = _content };

            btnSubmit = new Button() { Text = "SUBMIT", BackColor = SystemColors.Control };
            //btnSubmit.Click += (se, ev) => submit_Form();
            boxFooter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                BackColor = Color.Gray,
                AutoScroll = false,
                Padding = new Padding(0),
                FlowDirection = FlowDirection.RightToLeft,
            };
            boxFooter.Controls.AddRange(new Control[] { btnSubmit });

            this.Controls.AddRange(new Control[] { boxFooter, txtContent, boxWord });
            txtContent.BringToFront();
        }
    }

    public class FormELWordAdd : Form
    {
        #region [ === VARIABLE === ]

        const string _Word = "Word";
        const string _Meaning = "Meaning";

        private int _indexWord = -1;
        private int _indexMeaning = -1;

        private readonly string[] _urlResourceMp3 = new string[] { };
        private readonly string[] _fields = new string[] { _Word, "Pronounce", "Type", _Meaning, "Example", "Example_Meaning" };
        private readonly Dictionary<int, string> _indexField = new Dictionary<int, string>() { };
        private List<string[]> _listItems = new List<string[]>() { };
        private List<ELWORD> _listWord = new List<ELWORD>() { };
        private List<ELEXAMPLE> _listExam = new List<ELEXAMPLE>() { };

        private readonly int _form_Width = Screen.PrimaryScreen.WorkingArea.Width - 20;
        private readonly ListView view;
        private readonly Panel boxHeader;
        private readonly FlowLayoutPanel boxFooter;
        private readonly Button btnSubmit;
        private readonly Button btnFormat;
        private readonly Button btnUrlResourceMp3;
        private readonly CheckBox checkAll;

        #endregion

        public FormELWordAdd(List<string[]> _items, string[] urlResourceMp3)
        {
            btnUrlResourceMp3 = new Button() { Visible = false, BackColor = SystemColors.Control };
            if (urlResourceMp3 != null && urlResourceMp3.Length > 0)
            {
                btnUrlResourceMp3.Click += (se, ev) => show_MP3();
                btnUrlResourceMp3.Visible = true;
                btnUrlResourceMp3.Text = string.Format("MP3 ({0})", urlResourceMp3.Length);
                _urlResourceMp3 = urlResourceMp3;
            }

            _listItems = _items;

            this.Font = App.Font;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Left = 0;
            this.ClientSize = new System.Drawing.Size(_form_Width, 400);

            boxHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.Gray,
                AutoScroll = false,
                Padding = new Padding(0),
            };
            checkAll = new CheckBox() { Text = "All", Top = 7, Left = 3, Height = 18, Width = 37, };
            boxHeader.Controls.Add(checkAll);

            btnFormat = new Button() { Text = "FORMAT", BackColor = SystemColors.Control };
            btnFormat.Click += (se, ev) => format_Items();
            checkAll.CheckedChanged += (se, ev) => check_uncheck_All();

            btnSubmit = new Button() { Text = "SUBMIT", BackColor = SystemColors.Control };
            btnSubmit.Click += (se, ev) => submit_Form();
            boxFooter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                BackColor = Color.Gray,
                AutoScroll = false,
                Padding = new Padding(0),
                FlowDirection = FlowDirection.RightToLeft,
            };
            boxFooter.Controls.AddRange(new Control[] { btnSubmit, btnFormat, btnUrlResourceMp3 });

            view = new ListView()
            {
                CheckBoxes = true,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,

                HeaderStyle = ColumnHeaderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(12, 12),
                MultiSelect = false,
                Size = new Size(288, 303),
                UseCompatibleStateImageBehavior = false,
            };
            this.Controls.AddRange(new Control[] { boxHeader, view, boxFooter });
            view.BringToFront();

            load_Data();
        }

        private void load_Data()
        {
            view.Clear();
            int countCell = _listItems.Select(x => x.Length).Max();

            view.Columns.Add(new ColumnHeader { Text = "", Width = 40 });
            for (int i = 1; i < countCell; i++)
            {
                _indexField.Add(i, "");
                view.Columns.Add(new ColumnHeader { Text = "Column " + i.ToString(), Width = 125 });
                ComboBox com = new ComboBox()
                {
                    Tag = i,
                    Left = (i - 1) * 125 + 40,
                    Top = 7,
                    Width = 125,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                com.SelectedIndexChanged += (se, ev) =>
                {
                    ComboBox si = (ComboBox)se;
                    int column = (int)si.Tag;
                    string field = _fields[si.SelectedIndex == 0 ? 0 : si.SelectedIndex - 1];
                    _indexField[column] = field;
                    switch (field)
                    {
                        case _Word:
                            _indexWord = column;
                            break;
                        case _Meaning:
                            _indexMeaning = column;
                            break;
                    }
                };
                com.Items.Add("");
                com.Items.AddRange(_fields);
                boxHeader.Controls.Add(com);
                if (countCell == 3)
                {
                    if (i == 1)
                        com.SelectedIndex = 1;
                    else if (i == 2)
                        com.SelectedIndex = 4;
                }
                else if (i <= 3)
                    com.SelectedIndex = i;
            }

            int k = 1;
            foreach (string[] it in _listItems)
            {
                if (it.Length < 3) continue;
                bool chk = false;
                if (it[2][0] == '/') chk = true;
                it[1] = DataExtract.format_Word(it[1]);
                view.Items.Add(new ListViewItem(it) { Checked = chk });
                if (k % 2 == 0) view.Items[k - 1].BackColor = SystemColors.ControlLight;
                k++;
            }//end for line


        }// end function

        private void submit_Form()
        {



        }//end function

        private void format_Items()
        {
            var lsField = _indexField.Values.ToList();
            if (lsField.IndexOf(_Word) == -1 || lsField.IndexOf(_Meaning) == -1)
            {
                MessageBox.Show(string.Format("Please choose config column [{0}] and [{1}]", _Word, _Meaning));
                return;
            }

            int countCell = _listItems.Select(x => x.Length).Max();
            int colMiss = countCell - _indexField.Count;
            if (colMiss > 1)
            {
                if (MessageBox.Show("Has " + colMiss.ToString() + " column not config, Are you will submit?", "Are you sure ?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                    return;
            }

            int index = 0;
            _listWord.Clear();
            _listExam.Clear();

            #region
            foreach (string[] it in _listItems)
            {
                string word = DataExtract.format_Word(it[_indexWord]),
                    meaning = DataExtract.format_Meaning(it[_indexMeaning]);

                ELWORD o = new ELWORD() { WORD = word, MEANING = meaning };
                ELEXAMPLE xam = null;

                foreach (var kv in _indexField)
                {
                    switch (kv.Value)
                    {
                        case "Pronounce":
                            o.PRONOUNCE = it[kv.Key];
                            break;
                        case "Type":
                            o.TYPE = (byte)DataExtract.fomart_Type(it[kv.Key]);
                            break;
                        case "Example":
                            if (xam == null) xam = new ELEXAMPLE() { WORD = word };
                            xam.EXAMPLE = it[kv.Key];
                            break;
                        case "Example_Meaning":
                            if (xam == null) xam = new ELEXAMPLE() { WORD = word };
                            xam.MEANING = it[kv.Key];
                            break;
                    }
                }
                _listWord.Add(o);
                if (xam != null) _listExam.Add(xam);
                index++;
            }
            #endregion

        }

        private void check_uncheck_All()
        {
            bool chk = checkAll.Checked;
            for (int i = 0; i < view.Items.Count; i++)
                view.Items[i].Checked = chk;
        }

        private void show_MP3()
        {
            Form f = new Form() { Font = App.Font, ClientSize = new Size(1000, 400) };
            string mp3 = string.Join(Environment.NewLine, _urlResourceMp3);
            TextBox txt = new TextBox() { Dock = DockStyle.Fill, Multiline = true, Text = mp3, ScrollBars = ScrollBars.Vertical, WordWrap = false };
            f.Controls.Add(txt);
            f.ShowDialog();
        }
    }//end class

    public class FormELGramAdd : Form
    {
        private readonly FlowLayoutPanel boxWord;
        private readonly TextBox txtContent;
        private readonly FlowLayoutPanel boxFooter;
        private readonly Button btnSubmit;
        public FormELGramAdd(string text, string[] urlResourceMp3)
        {
            this.Font = App.Font;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(800, 500);

            string[] a = text.Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            string[] _words = a[0].ToUpper().Split(' ').Select(x => new String(x.ToCharArray().Where(Char.IsLetter).ToArray())).Where(x => x != "").ToArray();
            string _content = string.Join(Environment.NewLine, a.Where((x, k) => k > 0).ToArray());

            boxWord = new FlowLayoutPanel() { Dock = DockStyle.Top, Height = 22, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10, 3, 0, 0), };
            foreach (string w in _words)
                boxWord.Controls.Add(new Label() { Text = w, BackColor = Color.LightPink, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(10, 0, 10, 0), });

            txtContent = new TextBox() { Dock = DockStyle.Fill, Multiline = true, WordWrap = true, ScrollBars = ScrollBars.Vertical, Text = _content };

            btnSubmit = new Button() { Text = "SUBMIT", BackColor = SystemColors.Control };
            //btnSubmit.Click += (se, ev) => submit_Form();
            boxFooter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                BackColor = Color.Gray,
                AutoScroll = false,
                Padding = new Padding(0),
                FlowDirection = FlowDirection.RightToLeft,
            };
            boxFooter.Controls.AddRange(new Control[] { btnSubmit });

            this.Controls.AddRange(new Control[] { boxFooter, txtContent, boxWord });
            txtContent.BringToFront();
        }
    }

    public class DataExtract
    {

        public static string format_TextPlain(string htm)
        {
            string text = "";

            //// Loading HTML Page into DOM Object
            //IHTMLDocument2 doc = new HTMLDocumentClass();
            //doc.clear();
            //doc.write(htm);
            //doc.close();

            ////' The following is a must do, to make sure that the data is fully load.
            //while (doc.readyState != "complete")
            //{
            //    //This is also a important part, without this DoEvents() appz hangs on to the “loading”
            //    //System.Windows.Forms.Application.DoEvents(); 
            //    ;
            //} 
            //string source = doc.body.outerHTML; 
            //text = doc.body.innerText.Trim(); 


            text = new HtmlToText().ConvertToTextPlain(htm);
            return text;
        }

        public static List<string[]> format_TextHtml(string htm)
        {
            string text = format_TextPlain(htm);
            var li = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .Where(x => x != "")
                .Select((x, k) => (k + 1).ToString() + "\t" + x)
                .Select(x => x.Split(new string[] { ":", "\t" }, StringSplitOptions.None))
                .ToList();
            return li;
        }

        public static List<string[]> format_TableHTML(string htm)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htm);
            foreach (var eachNode in htmlDocument.DocumentNode.SelectNodes("//*"))
                eachNode.Attributes.RemoveAll();
            htm = htmlDocument.DocumentNode.OuterHtml;

            htm = "<table>" + string.Join(Environment.NewLine,
                htm.Split(new string[] { "<tr>" }, StringSplitOptions.None)
                .Select(x => "<tr>" + x.Trim()).Where(x => x.EndsWith("</tr>")).ToArray()) + "</table>";

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htm);

            var nodes = doc.DocumentNode.SelectNodes("//table/tr");
            var table = new DataTable("MyTable");

            List<string[]> lsTD = new List<string[]>();

            int index = 0;
            foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//table/tr"))
            {
                List<string> li = new List<string>();
                li.Add(index.ToString());

                foreach (HtmlNode col in row.SelectNodes("td"))
                    li.Add(col.InnerText.Trim());

                if (li.Count > 2)
                {
                    lsTD.Add(li.ToArray());
                    index++;
                }
            }
            return lsTD;
        }

        public static string format_Word(string s)
        {
            string v = s.Trim();
            string[] a = v.Split(new string[] { ".", ",", "-", "–" }, StringSplitOptions.None);
            if (a.Length > 1) v = a[1].Trim();
            return v.Split('(')[0].Trim();
        }

        public static string format_Meaning(string s)
        {
            string v = s.Trim();
            if (v.Length > 0 && v[0] == '–') v = v.Substring(1).Trim();
            return v;
        }

        private static ELTYPE[] _ELTypes = Enum.GetValues(typeof(ELTYPE)).OfType<ELTYPE>().ToArray();
        public static ELTYPE fomart_Type(string s)
        {
            ELTYPE type = ELTYPE.NONE;
            try
            {
                string key = String.Join("", s.Where(c => Char.IsLetter(c)).Select(c => c.ToString()).ToArray()).ToUpper();
                type = _ELTypes.Where(x => x.ToString() == key).SingleOrDefault();
            }
            catch { }
            return type;
        }
    }
}
