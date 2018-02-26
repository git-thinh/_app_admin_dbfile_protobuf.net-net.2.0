using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections;
using app.GUI.Html;

namespace app.GUI
{
    public partial class ucDataItemEdit : UserControl
    {
        public const int Height_Min = 20;
        public const int Height_Max = 60;
        public const int Height_Full = 350;

        public int _Height { private set; get; }
        public int _Width { private set; get; }
        public ucDataItemEdit(string dbName, int index, FieldInfo field, IDataFile db, IDataFieldEvent _fe)
        {
            int wi_Name = 110, wi_Max = 630, wi_ = 200;
            _Height = Height_Min;
            _Width = wi_ + wi_Name + 8;
            string caption = string.IsNullOrEmpty(field.CAPTION) ? field.NAME : field.CAPTION;
            Label lbl_Name = new Label() { Dock = DockStyle.Left, Left = 4, Top = 0, Text = caption, AutoSize = false, Width = wi_Name, ForeColor = Color.Black, TextAlign = ContentAlignment.TopRight };
            this.Controls.Add(lbl_Name);

            if (field.TYPE_NAME == typeof(Byte).Name && field.KIT != ControlKit.CHECK)
            {
                ComboBox cbo = new ComboBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field, };
                for (int k = 0; k < 256; k++) cbo.Items.Add(k.ToString());
                cbo.SelectedIndex = 0;
                this.Controls.Add(cbo);
                cbo.BringToFront();
                return;
            }

            #region

            switch (field.KIT)
            {
                case ControlKit.LABEL:
                    Label lbl = new Label() { Dock = DockStyle.Left, Left = wi_Name + 8, AutoSize = false, Width = wi_, Height = _Height - 5, BackColor = SystemColors.Control, Tag = field, };
                    this.Controls.Add(lbl);
                    if (field.Value != null) lbl.Text = field.Value.ToString();
                    lbl.BringToFront();
                    break;
                case ControlKit.CHECK:
                    CheckBox chk = new CheckBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Tag = field, };
                    this.Controls.Add(chk);
                    if (field.Value != null && field.Value.ToString() == "1") chk.Checked = true;
                    chk.BringToFront();
                    break;
                case ControlKit.RADIO:
                    RadioButton radio = new RadioButton() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Tag = field, };
                    this.Controls.Add(radio);
                    radio.BringToFront();
                    break;
                case ControlKit.COLOR:
                    Label lbl_Color = new Label() { Dock = DockStyle.Left, Left = wi_Name + 8, AutoSize = false, Width = 44, Height = _Height - 5, BackColor = Color.Gray, Tag = field, };
                    this.Controls.Add(lbl_Color);
                    if (field.Value != null) lbl_Color.Text = field.Value.ToString();
                    lbl_Color.BringToFront();
                    break;
                case ControlKit.SELECT:
                    #region

                    ComboBox cbo = new ComboBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field, };
                    IList lsi = db.GetComboboxItem(field);
                    if (lsi != null && lsi.Count > 0)
                    {
                        List<string> lf = new List<string>();
                        foreach (object it in lsi)
                        {
                            object v = it.GetType().GetProperty(field.JOIN_FIELD).GetValue(it, null);
                            lf.Add(v == null ? "" : v.ToString());
                        }

                        List<string> lv = new List<string>();
                        foreach (object it in lsi)
                        {
                            object v = it.GetType().GetProperty(field.JOIN_VIEW).GetValue(it, null);
                            lv.Add(v == null ? "" : v.ToString());
                        }

                        for (int k = 0; k < lv.Count; k++)
                            cbo.Items.Add(new ComboboxItem() { Text = lv[k], Value = lf[k] });
                    }

                    this.Controls.Add(cbo);

                    if (field.Value != null)
                    {
                        int kii = 0, _index = -1;
                        foreach (object myObj in lsi)
                        {
                            var lii = myObj.GetType().GetProperties().Select(pi => pi.GetValue(myObj, null)).Select(x => x == null ? "" : x.ToString()).ToList();
                            if (lii.IndexOf(field.Value.ToString()) != -1)
                            {
                                _index = kii;
                                break;
                            }
                            kii++;
                        }
                        if (_index != -1)
                            cbo.SelectedIndex = _index;
                    }

                    cbo.BringToFront();

                    #endregion
                    break;
                case ControlKit.TEXT_PASS:
                    TextBox txt_Pass = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, PasswordChar = '*', BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    if (field.Value != null) txt_Pass.Text = field.Value.ToString();
                    this.Controls.Add(txt_Pass);
                    txt_Pass.BringToFront();
                    break;
                case ControlKit.TEXT_DATE:
                case ControlKit.TEXT_DATETIME:
                case ControlKit.TEXT_TIME:
                    DateTimePicker dt = new DateTimePicker() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, CustomFormat = "dd-MM-yyyy HH:mm:ss", Format = DateTimePickerFormat.Custom, Tag = field, };
                    this.Controls.Add(dt);
                    dt.BringToFront();
                    break;
                case ControlKit.TEXT_EMAIL:
                    TextBox txt_Email = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    if (field.Value != null) txt_Email.Text = field.Value.ToString();
                    this.Controls.Add(txt_Email);
                    txt_Email.BringToFront();
                    break;
                case ControlKit.TEXT_FILE:
                    TextBox txt_File = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_ - 30, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    Button btn_Browser = new Button() { Dock = DockStyle.Left, Left = wi_Name + 28, Top = 0, Height = 18, Width = 30, Text = "...", FlatStyle = FlatStyle.Flat };
                    this.Controls.AddRange(new Control[] { txt_File, btn_Browser });
                    if (field.Value != null) txt_File.Text = field.Value.ToString();
                    txt_File.BringToFront();
                    btn_Browser.BringToFront();
                    btn_Browser.Click += (se, ev) => select_File(txt_File);
                    break;
                case ControlKit.TEXTAREA:
                    _Height = Height_Max;
                    _Width = wi_Max + wi_Name + 8;
                    TextBox text_area = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_Max, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Tag = field, };
                    this.Controls.Add(text_area);
                    if (field.Value != null) text_area.Text = field.Value.ToString();
                    text_area.BringToFront();
                    break;
                case ControlKit.LOOKUP:
                    TextBox txt_Lookup = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_Lookup);
                    if (field.Value != null) txt_Lookup.Text = field.Value.ToString();
                    txt_Lookup.BringToFront();
                    break;
                case ControlKit.HTML:
                    _Height = Height_Full;
                    _Width = wi_Max + wi_Name + 8;
                    HtmlEditor html = new HtmlEditor(db, _fe) { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_Max, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Tag = field, };
                    this.Controls.Add(html);
                    if (field.Value != null) html.Html = field.Value.ToString();
                    html.BringToFront();
                    break;
                default: //case ControlKit.TEXT: break;
                    TextBox txt = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt);
                    if (field.Value != null) txt.Text = field.Value.ToString();
                    txt.BringToFront();
                    break;
            }
            #endregion 
        }

        private string PathDirCurrent = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //private string PathDirCurrent = System.AppDomain.CurrentDomain.BaseDirectory;
        private void select_File(TextBox txt)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = PathDirCurrent;
            //openFile.Filter = "XML files (*.xml;*.html)|*.xml;*.html";
            openFile.Filter = "Files (*.*)|*.*";
            openFile.Title = "Select file";
            openFile.FilterIndex = 2;
            openFile.RestoreDirectory = true;
            //openFile.Multiselect = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txt.Text = openFile.FileName;
            }
        }
    }

}


