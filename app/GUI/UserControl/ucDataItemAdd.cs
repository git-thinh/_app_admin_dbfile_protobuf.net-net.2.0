using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using app.Core;
using System.Drawing;
using System.Collections;
using app.GUI.Html;

namespace app.GUI
{

    public partial class ucDataItemAdd : UserControl
    {
        public const int Height_Min = 20;
        public const int Height_Max = 60;
        public const int Height_Full = 350;

        public int _Height { private set; get; }
        public int _Width { private set; get; }
        public ucDataItemAdd(string dbName, int index, FieldInfo field, IDataFile db, IDataFieldEvent _fe)
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
                    lbl.BringToFront();
                    break;
                case ControlKit.CHECK:
                    CheckBox chk = new CheckBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Tag = field, };
                    this.Controls.Add(chk);
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
                    lbl_Color.BringToFront();
                    break;
                case ControlKit.SELECT:
                    ComboBox cbo = new ComboBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field, };
                    IList lsi = db.GetComboboxItem(field);
                    if (lsi != null && lsi.Count > 0)
                    {
                        cbo.DisplayMember = field.JOIN_VIEW;
                        cbo.ValueMember = field.JOIN_FIELD;
                        cbo.DataSource = lsi;
                    }
                    this.Controls.Add(cbo);
                    cbo.BringToFront();
                    break;
                case ControlKit.TEXT_PASS:
                    TextBox txt_Pass = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, PasswordChar = '*', BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
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
                    this.Controls.Add(txt_Email);
                    txt_Email.BringToFront();
                    break;
                case ControlKit.TEXT_FILE:
                    TextBox txt_File = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_ - 30, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    Button btn_Browser = new Button() { Dock = DockStyle.Left, Left = wi_Name + 28, Top = 0, Height = 18, Width = 30, Text = "...", FlatStyle = FlatStyle.Flat };
                    this.Controls.AddRange(new Control[] { txt_File, btn_Browser });
                    txt_File.BringToFront();
                    btn_Browser.BringToFront();
                    btn_Browser.Click += (se, ev) => select_File(txt_File);
                    break;
                case ControlKit.TEXTAREA:
                    _Height = Height_Max;
                    _Width = wi_Max + wi_Name + 8;
                    TextBox text_area = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_Max, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Tag = field, };
                    this.Controls.Add(text_area);
                    text_area.BringToFront();
                    break;
                case ControlKit.LOOKUP:
                    TextBox txt_Lookup = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_Lookup);
                    txt_Lookup.BringToFront();
                    break;
                case ControlKit.HTML:
                    _Height = Height_Full;
                    _Width = wi_Max + wi_Name + 8;
                    HtmlEditor html = new HtmlEditor(db, _fe) { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_Max, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Tag = field, };
                    this.Controls.Add(html);
                    html.BringToFront();
                    break;
                default: //case ControlKit.TEXT: break;
                    TextBox txt = new TextBox() { Dock = DockStyle.Left, Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt);
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
