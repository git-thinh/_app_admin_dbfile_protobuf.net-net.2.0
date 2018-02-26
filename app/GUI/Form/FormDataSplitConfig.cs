using System;
using System.Collections.Generic;
using System.Text;
using app.Core;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections;
using app.Model;

namespace app.GUI
{
    public class FormDataSplitConfig : FormBase
    {
        #region [  === VARIABLE ===  ]

        public delegate void EventSubmit(CNSPLIT config, string htm, string content);
        public EventSubmit OnSubmit;

        const int fWidth = 999;
        const int fHeight = 550;

        private TextBoxCustom txt_Domain;
        private TextBoxCustom txt_URL;
        private ucDataSplitConfig txt_TextFirst;
        private ucDataSplitConfig txt_TextLast;
        private ListBox list_TextFirst;
        private ListBox list_TextLast;

        private TextBoxCustom txt_Content;
        private IDataFile db;
        private string Domain = "";
        private string Content = "";

        #endregion

        public FormDataSplitConfig(IDataFile _db, string url, string content_ = "")
            : base("CONFIG", true)
        {
            db = _db;
            Domain = url.getDomainFromURL();
            if (content_ != "") Content = content_;
            else
                Content = url.getContentTextFromURL();
            ClientSize = new System.Drawing.Size(fWidth, fHeight);

            #region [ === UI === ]

            Panel main = new Panel() { BackColor = Color.WhiteSmoke, Dock = DockStyle.Fill, Padding = new Padding(10) };

            Panel box_Domain = new Panel() { Height = 30, Dock = DockStyle.Top, Padding = new Padding(0, 5, 0, 5) };
            Label lbl_Domain = new Label() { Width = 100, Dock = DockStyle.Left, Text = "Domain", TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 10, 0) };
            txt_Domain = new TextBoxCustom() { Text = url.getDomainFromURL(), Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            box_Domain.Controls.AddRange(new Control[] { lbl_Domain, txt_Domain });
            txt_Domain.BringToFront();

            Panel box_URL = new Panel() { Height = 30, Dock = DockStyle.Top, Padding = new Padding(0, 5, 0, 5) };
            Label lbl_URL = new Label() { Width = 100, Dock = DockStyle.Left, Text = "URL", TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 10, 0) };
            txt_URL = new TextBoxCustom() { Text = url, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            box_URL.Controls.AddRange(new Control[] { txt_URL, lbl_URL });

            Panel box_Content = new Panel() { Height = 300, Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5) };
            Label lbl_Content = new Label() { Width = 100, Dock = DockStyle.Left, Text = "Content", TextAlign = ContentAlignment.TopRight, Padding = new Padding(0, 0, 10, 0) };
            txt_Content = new TextBoxCustom() { Text = Content, Tag = Content, Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = true, BorderStyle = BorderStyle.FixedSingle };
            box_Content.Controls.AddRange(new Control[] { txt_Content, lbl_Content });

            Panel box_Text = new Panel() { Dock = DockStyle.Bottom, Height = 120 };
            Panel box_TextFirst = new Panel() { Dock = DockStyle.Right, Width = fWidth / 2 - 200 };
            txt_TextFirst = new ucDataSplitConfig("___ Text First ___", "Title") { Dock = DockStyle.Top, Height = 20 };
            list_TextFirst = new ListBox() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, };
            box_TextFirst.Controls.AddRange(new Control[] { list_TextFirst, txt_TextFirst });

            Panel box_TextLast = new Panel() { Dock = DockStyle.Right, Width = fWidth / 2 - 200 };
            txt_TextLast = new ucDataSplitConfig("___ Text Last ___", "Signature") { Dock = DockStyle.Top, Height = 20 };
            list_TextLast = new ListBox() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, };
            box_TextLast.Controls.AddRange(new Control[] { list_TextLast, txt_TextLast });
            
            Button btn_TextFirst_Add = new Button() { Text = "Add Config", Dock = DockStyle.Right, Width = 50 };
            Button btn_TextFirst_Remove = new Button() { Text = "Remove Config", Dock = DockStyle.Right, Width = 60 };
            Button btn_TextTestConfig = new Button() { Text = "Test Config", Dock = DockStyle.Right, Width = 60 };
            Button btn_TextResetContent = new Button() { Text = "Reset Content", Dock = DockStyle.Right, Width = 60 };

            box_Text.Controls.AddRange(new Control[] {
                btn_TextTestConfig,
                new Label(){ Dock = DockStyle.Right, AutoSize = false, Width = 10, Height = 40},
                btn_TextResetContent,
                new Label(){ Dock = DockStyle.Right, AutoSize = false, Width = 10, Height = 40},
                box_TextFirst,
                new Label(){ Dock = DockStyle.Right, AutoSize = false, Width = 10, Height = 40},
                box_TextLast,
                new Label(){ Dock = DockStyle.Right, AutoSize = false, Width = 10, Height = 40},
                btn_TextFirst_Add,
                new Label(){ Dock = DockStyle.Right, AutoSize = false, Width = 10, Height = 10},
                btn_TextFirst_Remove,
            });

            Panel box_Footer = new Panel() { Height = 30, Dock = DockStyle.Bottom, Padding = new Padding(0, 10, 0, 0) };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT" };
            box_Footer.Controls.AddRange(new Control[] { btn_Submit });

            main.Controls.AddRange(new Control[] { box_Footer, box_Text, box_Content, box_URL, box_Domain });
            this.Controls.Add(main);
            box_Footer.SendToBack();
            box_Content.BringToFront();
            this.PanelHeader_SendToBack();

            #endregion

            list_TextFirst.Format += listBox_Format;
            list_TextLast.Format += listBox_Format;

            SearchResult rs = db.FindItemByContainFieldValue(new CNSPLIT() { SITE = Domain }, "SITE", 1000, 1);
            if (rs != null && rs.Status && rs.Total > 0)
            {
                var a = ((IList)rs.Message).Convert<CNSPLIT>();
                var af = a.Select(x => x.SKIP_LINE_TOP.ToString() + " | " + x.TEXT_FIRST).ToArray();
                var al = a.Select(x => x.SKIP_LINE_BOTTOM.ToString() + " | " + x.TEXT_LAST).ToArray();
                 
                var lks = a.AsQueryable().Select<long>(rs.FieldSyncEdit).Cast<long>().ToArray();

                for (int i = 0; i < lks.Length; i++)
                {
                    list_TextFirst.Items.Add(new ComboboxItem() { Text = af[i], Value = lks[i] });
                    list_TextLast.Items.Add(new ComboboxItem() { Text = al[i], Value = lks[i] });
                }

                //list_TextFirst.Items.AddRange(af);
                //list_TextLast.Items.AddRange(al);
            }

            #region [ === EVENT === ] 

            list_TextFirst.SelectedIndexChanged += (se, ev) => list_Split_Choose(list_TextFirst.SelectedIndex);
            list_TextLast.SelectedIndexChanged += (se, ev) => list_Split_Choose(list_TextLast.SelectedIndex);

            btn_TextFirst_Add.Click += (se, ev) => config_Add();
            btn_TextFirst_Remove.Click += (se, ev) => config_Remove();
            btn_TextResetContent.Click += (se, ev) => { txt_Content.Text = txt_Content.Tag as string; };
            btn_TextTestConfig.Click += (se, ev) => config_Test();
            btn_Submit.Click += (se, ev) => submit();

            #endregion
        }

        private void listBox_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is ComboboxItem)
            {
                e.Value = ((ComboboxItem)e.ListItem).Text;
            }
            else
            {
                e.Value = "Unknown item added";
            }
        }

        void list_Split_Choose(int index)
        {
            if (index == -1) return;
            txt_TextFirst.Value = new ConfigItem(list_TextFirst.Items[index].ToString());
            txt_TextLast.Value = new ConfigItem(list_TextLast.Items[index].ToString());
        }

        private void submit()
        {
            if (string.IsNullOrEmpty(Content))
            {
                MessageBox.Show("Crawler config fail. Can not extract content from html");
                return;
            }

            if (OnSubmit != null) OnSubmit(new CNSPLIT()
            {
                SITE = Domain,
                TEXT_FIRST = txt_TextFirst.Value.Text,
                TEXT_LAST = txt_TextLast.Value.Text,
                SKIP_LINE_BOTTOM = (byte)txt_TextLast.Value.SkipLine,
                SKIP_LINE_TOP = (byte)txt_TextFirst.Value.SkipLine,
            }, txt_Content.Tag as string, Content);
        }

        private void config_Add()
        {
            ConfigItem first = txt_TextFirst.Value;
            ConfigItem last = txt_TextLast.Value;
            if (first == null || last == null || string.IsNullOrEmpty(first.Text) || string.IsNullOrEmpty(last.Text))
            {
                MessageBox.Show("Please input text first and last config.");
                return;
            }

            IndexDynamic id = db.FindItemFirstByContainFieldValue(new CNSPLIT() { SITE = Domain, TEXT_FIRST = first.Text, TEXT_LAST = last.Text }, "SITE,TEXT_FIRST,TEXT_LAST");
            if (id.Index == -1 || id.Item == null)
            {
                object o = new CNSPLIT()
                {
                    SITE = Domain,
                    TEXT_FIRST = first.Text,
                    TEXT_LAST = last.Text,
                    SKIP_LINE_BOTTOM = (byte)last.SkipLine,
                    SKIP_LINE_TOP = (byte)first.SkipLine,
                };
                object add = db.AddItem(o);
                if (add != null && add.GetType().Name != typeof(EditStatus).Name)
                {
                    CNSPLIT val = add.Convert<CNSPLIT>();
                    list_TextFirst.Items.Insert(0, new ComboboxItem() { Value = val.ID, Text = first.SkipLine.ToString() + " | " + first.Text });
                    list_TextLast.Items.Insert(0, new ComboboxItem() { Value = val.ID, Text = last.SkipLine.ToString() + " | " + last.Text });

                    txt_TextFirst.Reset();
                    txt_TextLast.Reset();
                }
                else
                    MessageBox.Show("Add object fail");
            }
            else
                MessageBox.Show("Config exist");
        }

        private void config_Remove()
        {
            int index = list_TextFirst.SelectedIndex == -1 ? list_TextLast.SelectedIndex : list_TextFirst.SelectedIndex;            
            if (index == -1)
            {
                MessageBox.Show("Please choose on list box text to remove it");
                return;
            }

            if (MessageBox.Show("You will remove config choose ?", "Are you sure ?",
            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                long id = ((long)(list_TextFirst.Items[index] as ComboboxItem).Value);

                bool val = db.RemoveItemByKeyFieldSyncEdit(typeof(CNSPLIT).Name, id);
                if (val)
                {
                    list_TextFirst.Items.RemoveAt(index);
                    list_TextLast.Items.RemoveAt(index);
                    //txt_TextFirst.Reset();
                    //txt_TextLast.Reset();
                }
                else
                    MessageBox.Show("Remove object fail");
            }
        }

        private void config_Test()
        {
            ConfigItem first = txt_TextFirst.Value;
            ConfigItem last = txt_TextLast.Value;
            if (first == null || last == null || string.IsNullOrEmpty(first.Text) || string.IsNullOrEmpty(last.Text))
            {
                MessageBox.Show("Please input or choose text first and last config.");
                return;
            }

            IndexLine[] a = Content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x != "").Select((x, k) => new IndexLine(k, x)).ToArray();
            int pLast = a.getIndex(last, -1), pFirst = a.getIndex(first, pLast);
            if (pFirst == -1 || pLast == -1)
            {
                MessageBox.Show("Config wrong, can not find position first or last of data content");
                return;
            }
            string[] con = a.Where(x => x.Index > pFirst && x.Index < pLast).Select(x => x.Line).ToArray();
            txt_Content.Text = string.Join(Environment.NewLine, con);
        }


    }

}
