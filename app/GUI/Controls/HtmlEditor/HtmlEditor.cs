using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using mshtml;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using Microsoft.VisualBasic;
using app.Core;
using System.Collections;
using app.Model;

namespace app.GUI.Html
{
    public partial class HtmlEditor : UserControl
    {
        private readonly System.Windows.Forms.HtmlDocument _doc;
        private readonly IList<IHTMLEditorButton> _customButtons;

        private static readonly string[] _webSafeFonts = new[] { "Courier New", "Times New Roman", "Georgia", "Arial", "Verdana" };

        private readonly IDataFile db;
        private readonly IDataFieldEvent DataFieldEvent;
        public HtmlEditor(IDataFile _db, IDataFieldEvent _fe)
        {
            db = _db;
            DataFieldEvent = _fe;
            InitializeComponent();

            InitializeWebBrowserAsEditor();

            _doc = textWebBrowser.Document;
            _customButtons = new List<IHTMLEditorButton>();

            updateToolBarTimer.Start();
            updateToolBarTimer.Tick += updateToolBarTimer_Tick;

            this.AddToolbarItem(new BoldButton());
            this.AddToolbarItem(new ItalicButton());
            this.AddFontSelector(_webSafeFonts);
            this.AddFontSizeSelector(Enumerable.Range(1, 7));
            this.AddToolbarDivider();
            this.AddToolbarItem(new LinkButton());
            this.AddToolbarItem(new UnlinkButton());
            this.AddToolbarDivider();
            this.AddToolbarItem(new InsertLinkedImageButton());
            this.AddToolbarDivider();
            this.AddToolbarItem(new OrderedListButton());
            this.AddToolbarItem(new UnorderedListButton());
            this.AddToolbarDivider();
            this.AddToolbarItem(new ForecolorButton());
            this.AddToolbarDivider();
            this.AddToolbarItem(new JustifyLeftButton());
            this.AddToolbarItem(new JustifyCenterButton());
            this.AddToolbarItem(new JustifyRightButton());
            this.AddToolbarDivider();
            this.addButton_WebCopy();

            this.AddToolbarItem(btn_Clear);
            btn_Clear.Click += (se, ev) =>
            {
                this.Html = "";
            };
        }

        private ToolStripButton btn_Clear = new ToolStripButton() { Text = "Clear" };

        public CmsExtract cmsExtract { set; get; }

        public void FormatHTML()
        {

            this.Cursor = Cursors.WaitCursor;

            string s = this.Html;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(s);
            s = doc.DocumentNode.InnerHtml;
            var atts = doc.DocumentNode.SelectNodes("//*[@style or @class]").ToArray();
            foreach (HtmlNode n in atts)
            {
                string val = "";
                switch (n.Name)
                {
                    case "a":
                        val = n.InnerHtml.Trim();
                        break;
                    case "img":
                        string src = n.GetAttributeValue("src", "");
                        if (src != "")
                        {
                            if (src.StartsWith("http"))
                            {
                                var request = WebRequest.Create(src);
                                using (var response = request.GetResponse())
                                using (var stream = response.GetResponseStream())
                                {
                                    byte[] imageBytes;
                                    using (BinaryReader br = new BinaryReader(stream))
                                    {
                                        int len = (int)(response.ContentLength);
                                        imageBytes = br.ReadBytes(len);
                                        br.Close();
                                    }

                                    src = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                                }
                            }
                            val = string.Format(@"<{0} src=""{1}"" />", n.Name, src);
                        }
                        break;
                    default:
                        val = string.Format("<{0}>{1}</{0}>", n.Name, n.InnerHtml.Trim());
                        break;
                }
                s = s.Replace(n.OuterHtml, val);
            }

            this.Html = s;
            this.Cursor = Cursors.Default;
        }

        public void AddFormatButton()
        {
            var b = new ToolStripButton();
            b.Text = "Chuẩn hóa dữ liệu";
            b.Click += (se, ev) =>
            {
                FormatHTML();
            };
            this.AddToolbarItem(b);
        }

        public void addButton_WebCopy()
        {
            var b = new ToolStripButton();
            b.Image = ___HtmlEditorResource.GetImageIcon("web.gif");
            b.ToolTipText = "Copy from URL Website";
            b.Click += (se, ev) =>
            {
                string urc = Clipboard.GetText(TextDataFormat.Text);
                if (!string.IsNullOrEmpty(urc) && !urc.ToLower().StartsWith("http")) urc = "";
                var url = Interaction.InputBox("Please enter an site url", "URL", urc, 99, 99);
                if (!string.IsNullOrEmpty(url))
                {
                    // Set cursor as hourglass
                    Cursor.Current = Cursors.WaitCursor;

                    string dom = url.getDomainFromURL();
                    SearchResult rs = db.FindItemByContainFieldValue(new CNSPLIT() { SITE = dom }, "SITE", 1000, 1);
                    if (rs == null || rs.Total == 0)
                    {
                        var fc = new FormDataSplitConfig(db, url);
                        fc.OnSubmit += (config, htm, content) =>
                        {
                            CmsExtract cms = content.get_CmsExtract(url, config, htm);
                            this.Html = cms.ContentHtml;
                            this.cmsExtract = cms;
                            fillFormCMS(cms);
                            fc.Close();
                        };
                        fc.ShowDialog();
                    }
                    else
                    {
                        string Content = url.getContentTextFromURL();
                        var ls = ((IList)rs.Message).Convert<CNSPLIT>().ToArray();

                        CNSPLIT config = (CNSPLIT)ls[0];
                        IndexLine[] a = Content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x != "").Select((x, k) => new IndexLine(k, x)).ToArray();
                        //int pLast = a.getIndex(config.TEXT_LAST, -1), pFirst = a.getIndex(config.TEXT_FIRST, pLast);
                        int pLast = a.getIndex(new ConfigItem() { Text = config.TEXT_LAST, SkipLine = config.SKIP_LINE_BOTTOM }, -1),
                            pFirst = a.getIndex(new ConfigItem() { Text = config.TEXT_FIRST, SkipLine = config.SKIP_LINE_TOP }, pLast);
                        if (pFirst == -1 || pLast == -1)
                        {
                            MessageBox.Show("Config wrong, can not find position first or last of data content");
                            return;
                        }
                        string[] con = a.Where(x => x.Index > pFirst && x.Index < pLast).Select(x => x.Line).ToArray();
                        string data = string.Join(Environment.NewLine, con);

                        CmsExtract cms = data.get_CmsExtract(url, config, "");
                        this.Html = cms.ContentHtml;
                        this.cmsExtract = cms;
                        fillFormCMS(cms);
                    }

                    // Set cursor as default arrow
                    Cursor.Current = Cursors.Default;
                }
            };
            this.AddToolbarItem(b);
        }

        private void fillFormCMS(CmsExtract cms)
        {
            var data = new Dictionary<string, object>()
            {
                {"TITLE", cms.Title},
                {"IMG_DEFAULT", cms.ImgDefault},
                {"DESCRIPTION", cms.Description},
            };
            if (DataFieldEvent != null) DataFieldEvent.BindDataUC(data);
        }

        public void AddFontSizeSelector(IEnumerable<int> fontSizes)
        {
            if (fontSizes.Min() < 1 || fontSizes.Max() > 7)
            {
                throw new ArgumentException("Allowable font sizes are 1 through 7");
            }

            var fontSizeBox = new ToolStripComboBox();
            fontSizeBox.Items.AddRange(fontSizes.Select(f => f.ToString()).ToArray());
            fontSizeBox.Name = "fontSizeSelector";
            fontSizeBox.Size = new System.Drawing.Size(25, 25);
            fontSizeBox.SelectedIndexChanged += (sender, o) =>
            {
                var size = ((ToolStripComboBox)sender).SelectedItem;
                _doc.ExecCommand("FontSize", false, size);
            };
            fontSizeBox.DropDownStyle = ComboBoxStyle.DropDownList;

            this.AddToolbarItem(fontSizeBox);
        }

        public void AddFontSelector(IEnumerable<string> fontNames)
        {
            var dropDown = new ToolStripDropDownButton();
            foreach (var fontName in fontNames)
            {
                dropDown.DropDownItems.Add(GetFontDropDownItem(fontName));
            }
            dropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            dropDown.Name = "Font";
            dropDown.Size = new System.Drawing.Size(29, 22);
            dropDown.Text = "Font";

            this.AddToolbarItem(dropDown);
        }

        private ToolStripItem GetFontDropDownItem(string fontName)
        {
            var dropDownItem = new ToolStripMenuItem();
            dropDownItem.Font = new System.Drawing.Font(fontName, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dropDownItem.Name = "fontMenuItem" + Guid.NewGuid();
            dropDownItem.Size = new System.Drawing.Size(173, 22);
            dropDownItem.Text = fontName;
            dropDownItem.Click += (sender, e) => _doc.ExecCommand("FontName", false, fontName);
            return dropDownItem;
        }

        public void AddToolbarItem(ToolStripItem toolStripItem)
        {
            editcontrolsToolStrip.Items.Add(toolStripItem);
        }

        public void AddToolbarItems(IEnumerable<ToolStripItem> toolStripItems)
        {
            foreach (var stripItem in toolStripItems)
            {
                editcontrolsToolStrip.Items.Add(stripItem);
            }
        }

        public void AddToolbarItem(IHTMLEditorButton toolbarItem)
        {
            _customButtons.Add(toolbarItem);
            editcontrolsToolStrip.Items.Add(CreateButton(toolbarItem));
        }

        public void AddToolbarItems(IEnumerable<IHTMLEditorButton> toolbarItems)
        {
            foreach (var toolbarItem in toolbarItems)
            {
                AddToolbarItem(toolbarItem);
            }
        }

        public void AddToolbarDivider()
        {
            var divider = new ToolStripSeparator();
            divider.Size = new System.Drawing.Size(6, 25);
            editcontrolsToolStrip.Items.Add(divider);
        }

        private ToolStripItem CreateButton(IHTMLEditorButton toolbarItem)
        {
            var toolStripButton = new ToolStripButton();
            toolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton.Image = toolbarItem.IconImage;
            toolStripButton.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton.Name = toolbarItem.IconName;
            toolStripButton.Size = new System.Drawing.Size(25, 24);
            toolStripButton.Text = toolbarItem.IconTooltip;

            var args = new HTMLEditorButtonArgs();
            args.Document = _doc;
            args.Editor = this;

            IHTMLEditorButton button = toolbarItem;
            toolStripButton.Click += (sender, o) => button.IconClicked(args);

            return toolStripButton;
        }

        public bool ReadOnly
        {
            get
            {
                if (textWebBrowser.Document != null)
                {
                    var doc = textWebBrowser.Document.DomDocument as IHTMLDocument2;
                    if (doc != null)
                    {
                        return doc.designMode != "On";
                    }
                }
                return false;
            }
            set
            {
                if (textWebBrowser.Document != null)
                {
                    var designMode = value ? "Off" : "On";
                    var doc = textWebBrowser.Document.DomDocument as IHTMLDocument2;
                    if (doc != null) doc.designMode = designMode;
                }
            }
        }

        public bool ShowToolbar
        {
            get
            {
                if (editcontrolsToolStrip != null)
                {
                    return editcontrolsToolStrip.Visible;
                }
                return true;
            }
            set { editcontrolsToolStrip.Visible = value; }
        }

        private void updateToolBarTimer_Tick(object sender, System.EventArgs e)
        {
            CheckCommandStateChanges();
        }

        private void InitializeWebBrowserAsEditor()
        {
            // It is necessary to add a body to the control before you can apply changes to the DOM document
            textWebBrowser.DocumentText = "<html><body></body></html>";
            if (textWebBrowser.Document != null)
            {
                var doc = textWebBrowser.Document.DomDocument as IHTMLDocument2;
                if (doc != null) doc.designMode = "On";

                // replace the context menu for the web browser control so the default IE browser context menu doesn't show up
                textWebBrowser.IsWebBrowserContextMenuEnabled = false;
                if (this.ContextMenuStrip == null)
                {
                    textWebBrowser.Document.ContextMenuShowing += (sender, e) => {; };
                }
            }
        }

        void Document_ContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            this.ContextMenuStrip.Show(this, this.PointToClient(Cursor.Position));
        }

        public string getBodyHTML()
        {
            return textWebBrowser.Document.Body.InnerHtml;
        }

        public string Html
        {
            get
            {
                return textWebBrowser.DocumentText == null ? "" : textWebBrowser.DocumentText;
            }
            set
            {
                if (textWebBrowser.Document != null)
                {
                    // updating this way avoids an alert box
                    var doc = textWebBrowser.Document.DomDocument as IHTMLDocument2;
                    if (doc != null)
                    {
                        doc.write(value);
                        doc.close();
                    }
                }
            }
        }

        public string InsertTextAtCursor
        {
            set { _doc.ExecCommand("Paste", false, value); }
        }

        private void CheckCommandStateChanges()
        {
            var doc = (IHTMLDocument2)_doc.DomDocument;

            var commands = _customButtons.Select(c => c.CommandIdentifier).Where(c => !string.IsNullOrEmpty(c));

            foreach (var command in commands)
            {
                var button = (ToolStripButton)editcontrolsToolStrip.Items[command];

                if (button == null) continue;

                if (doc.queryCommandState(command))
                {
                    if (button.CheckState != CheckState.Checked)
                    {
                        button.Checked = true;
                    }
                }
                else
                {
                    if (button.CheckState == CheckState.Checked)
                    {
                        button.Checked = false;
                    }
                }
            }
        }

        private void HtmlEditor_ContextMenuStripChanged(object sender, System.EventArgs e)
        {
            if (textWebBrowser.Document != null)
            {
                textWebBrowser.Document.ContextMenuShowing += Document_ContextMenuShowing;
            }
        }
    }
}
