using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System;
using app.Model;

namespace app.GUI
{

    public class FormModelAdd : FormBase
    {
        const int fWidth = 800;
        const int fHeight = 500;

        public delegate void EventSubmit(DB_MODEL model);
        public EventSubmit OnSubmit;

        private readonly IDataFile db;

        public FormModelAdd(IDataFile _db)
            : base("Model Add", true)
        {
            db = _db;
            ClientSize = new Size(fWidth, fHeight);

            countField = 0;

            #region [ === CONTROLS UI === ]

            Panel boi_DbName = new Panel() { Dock = DockStyle.Top, Height = 62, };

            boi_DbName.MouseDown += FormMove_MouseDown;
            Label lbl_Name = new Label() { Left = 4, Width = 120, Top = 7, Text = "Model name", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            TextBoxCustom txt_Name = new TextBoxCustom() { Left = 124, Top = 7, Width = 120, WaterMark = "Model name ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            Label lbl_Caption = new Label() { Left = 252, Top = 7, Width = 120, Text = "Model caption", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            TextBoxCustom txt_Caption = new TextBoxCustom() { Left = 372, Top = 7, Width = 120, WaterMark = "Model caption ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            boi_DbName.Controls.AddRange(new Control[] { lbl_Name, lbl_Caption, txt_Name, txt_Caption ,
                new ucModelTitle() { Left = 4, Top = 39, Height = 25, Width = fWidth - (SystemInformation.VerticalScrollBarWidth + 20)} });

            FlowLayoutPanel boi_Filter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(0),
                BackColor = Color.WhiteSmoke,
                FlowDirection = FlowDirection.TopDown,
            };
            boi_Filter.MouseDown += FormMove_MouseDown;

            form_Add(boi_Filter);
            form_Add(boi_Filter);

            Panel boi_Action = new Panel() { Dock = DockStyle.Bottom, Height = 25 };
            Button btn_Add = new Button() { Dock = DockStyle.Right, Text = "ADD", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            //Button btn_Remove = new Button() { Dock = DockStyle.Right, Text = "REMOVE", BackColor = Color.WhiteSmoke, Width = 70, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            boi_Action.Controls.AddRange(new Control[] { btn_Add, btn_Submit });

            this.Controls.AddRange(new Control[] { boi_DbName, boi_Filter, boi_Action });
            boi_DbName.BringToFront();
            boi_Action.BringToFront();
            boi_Filter.BringToFront();
            btn_Submit.Focus();

            btn_Add.Click += (se, ev) => form_Add(boi_Filter);
            //btn_Remove.Click += (se, ev) => form_Remove(boi_Filter);

            #endregion

            btn_Submit.Click += (se, ev) => form_Submit(txt_Name.Text, txt_Caption.Text, boi_Filter);
            HideScrollBar(boi_Filter.Handle, ScrollBarHide.SB_HORZ);
        }//end function init()

        private int countField = 0;
        private void form_Add(FlowLayoutPanel boi_Filter)
        {
            countField++;
            var uc = new ucModelFieldAdd(countField, db) { Name = "uc" + countField.ToString(), Height = 35, Width = fWidth };
            //uc.OnRemoveField += (index) => form_RemoveAt(boi_Filter, index);
            boi_Filter.Controls.Add(uc);
        }

        //private void form_RemoveAt(FlowLayoutPanel boi_Filter, int index)
        //{
        //    if (index == countField)
        //        form_Remove(boi_Filter);
        //    else
        //    {
        //        listIndexRemove.Add(index);
        //        foreach (Control c in boi_Filter.Controls)
        //        {
        //            if (c.Name == "uc" + index.ToString())
        //            {
        //                c.Visible = false;
        //                break;
        //            }
        //        }
        //    }
        //}

        //private void form_Remove(FlowLayoutPanel boi_Filter)
        //{
        //    int id = countField;
        //    countField--;
        //    if (countField == -1) return;
        //    foreach (Control c in boi_Filter.Controls)
        //        if (c.Name == "uc" + id.ToString())
        //        {
        //            boi_Filter.Controls.Remove(c);
        //            break;
        //        }
        //}

        private void form_Submit(string dbName, string dbCaption, FlowLayoutPanel boi_Filter)
        {
            //string dbName = txt_Name.Text, dbCaption = txt_Caption.Text;
            if (string.IsNullOrEmpty(dbName))
            {
                MessageBox.Show("Please input Model Name and fields: name, type, caption.");
                return;
            }

            dbName = dbName.ToUpper().Trim();

            if (MessageBox.Show("Are you sure update model [" + dbName + "] ?", "Update Model",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            bool exist = db.ExistModel(dbName);
            if (exist)
            {
                MessageBox.Show("Model Name exist. Please choose other name.");
                return;
            }

            var li = new List<FieldInfo>();
            int index = 1;
            foreach (Control c in boi_Filter.Controls)
            {
                var o = new FieldInfo();
                int ki = 0;
                foreach (Control fi in c.Controls)
                {
                    #region
                    if (fi.Name == "name" + index.ToString())
                        o.NAME = (fi as TextBox).Text;
                    else if (fi.Name == "type" + index.ToString())
                    {
                        int ix = (fi as ComboBox).SelectedIndex;
                        o.TYPE_NAME = (fi as ComboBox).Items[ix].ToString();
                    }
                    else if (fi.Name == "auto" + index.ToString())
                    {
                        o.IS_KEY_AUTO = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "kit" + index.ToString())
                    {
                        #region

                        if (o.IS_KEY_AUTO) continue;

                        object _coltrol = (fi as ComboBox).SelectedItem;
                        if (_coltrol != null)
                        {
                            try
                            {
                                o.KIT = (ControlKit)((int)(_coltrol as ComboboxItem).Value);
                            }
                            catch { }
                        }

                        #endregion
                    }
                    else if (fi.Name == "link_type" + index.ToString())
                    {
                        if (o.IS_KEY_AUTO) continue;
                        o.JOIN_TYPE = JoinType.NONE;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                        {
                            try
                            {
                                o.JOIN_TYPE = (JoinType)((int)(ct as ComboboxItem).Value);
                            }
                            catch { }
                        }
                    }
                    else if (fi.Name == "value_default" + index.ToString())
                    {
                        if (o.IS_KEY_AUTO) continue;
                        string vd = (fi as TextBox).Text;
                        o.VALUE_DEFAULT = vd == null ? new string[] { } : vd.Split('|');
                    }
                    else if (fi.Name == "link_model" + index.ToString())
                    {
                        if (o.IS_KEY_AUTO) continue;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                            o.JOIN_MODEL = (ct as ComboboxItem).Value as string;
                    }
                    else if (fi.Name == "link_field" + index.ToString())
                    {
                        if (o.IS_KEY_AUTO) continue;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                            o.JOIN_FIELD = (ct as ComboboxItem).Value as string;
                    }
                    else if (fi.Name == "link_view" + index.ToString())
                    {
                        if (o.IS_KEY_AUTO) continue;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                            o.JOIN_VIEW = (ct as ComboboxItem).Value as string;
                    }
                    else if (fi.Name == "index" + index.ToString())
                    {
                        o.IS_INDEX = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "null" + index.ToString())
                    {
                        o.IS_ALLOW_NULL = (fi as CheckBox).Checked;
                        if (o.IS_KEY_AUTO || o.IS_INDEX) o.IS_ALLOW_NULL = false;
                    }
                    else if (fi.Name == "caption" + index.ToString())
                    {
                        o.CAPTION = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "caption_short" + index.ToString())
                    {
                        o.CAPTION_SHORT = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "des" + index.ToString())
                    {
                        o.DESCRIPTION = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "mobi" + index.ToString())
                    {
                        o.MOBI = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "tablet" + index.ToString())
                    {
                        o.TABLET = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "duplicate" + index.ToString())
                    {
                        o.IS_NOT_DUPLICATE = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "encrypt" + index.ToString())
                    {
                        o.IS_ENCRYPT = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "order_edit" + index.ToString())
                    {
                        int _vi = 0;
                        if (int.TryParse((fi as TextBox).Text, out _vi))
                            o.ORDER_EDIT = _vi;
                    }
                    else if (fi.Name == "order_view" + index.ToString())
                    {
                        int _vi = 0;
                        if (int.TryParse((fi as TextBox).Text, out _vi))
                            o.ORDER_VIEW = _vi;
                    }
                    else if (fi.Name == "func_edit" + index.ToString())
                    {
                        o.FUNC_EDIT = fi.Text == dbFunc.title_FUNC_VALIDATE_ON_FORM ? "": fi.Text;
                    }
                    else if (fi.Name == "func_before_update" + index.ToString())
                    {
                        o.FUNC_BEFORE_UPDATE = fi.Text == dbFunc.title_FUNC_BEFORE_ADD_OR_UPDATE ? "" : fi.Text;
                    }
                    else if (fi.Name == "key_url" + index.ToString())
                    {
                        o.ORDER_KEY_URL = string.IsNullOrEmpty(fi.Text) ? 0 : int.Parse(fi.Text);
                    }
                    else if (fi.Name == "show_in_grid" + index.ToString())
                    {
                        o.ONLY_SHOW_IN_DETAIL = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "full_text_search" + index.ToString())
                    {
                        o.IS_FULL_TEXT_SEARCH = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "key_for_sync" + index.ToString())
                    {
                        o.IS_KEY_SYNC_EDIT = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "field_change" + index.ToString())
                    {
                        try
                        {
                            o.FieldChange = string.IsNullOrEmpty(fi.Text) ? dbFieldChange.NONE : (dbFieldChange)int.Parse(fi.Text);
                        }
                        catch { }
                    }
                    ki++;
                    #endregion
                }//end for fields 

                if (!string.IsNullOrEmpty(o.NAME) && o.Type != null)
                {
                    #region
                    switch (o.KIT)
                    { 
                        case ControlKit.RADIO:
                            o.JOIN_TYPE = JoinType.DEF_VALUE;
                            if (o.VALUE_DEFAULT == null || o.VALUE_DEFAULT.Length == 0 || (o.VALUE_DEFAULT.Length == 1 && o.VALUE_DEFAULT[0] == ""))
                            {
                                MessageBox.Show("Please input field [ " + o.NAME + " ] attributed [ Value Default ]");
                                return;
                            }
                            break;
                        case ControlKit.SELECT:
                            if (o.JOIN_TYPE == JoinType.DEF_VALUE && (o.VALUE_DEFAULT == null || o.VALUE_DEFAULT.Length == 0 || (o.VALUE_DEFAULT.Length == 1 && o.VALUE_DEFAULT[0] == "")))
                            {
                                MessageBox.Show("Please input field [ " + o.NAME + " ] attributed [ Value Default ]");
                                return;
                            }
                            if (o.JOIN_TYPE == JoinType.JOIN_MODEL && (string.IsNullOrEmpty(o.JOIN_MODEL) || string.IsNullOrEmpty(o.JOIN_FIELD)))
                            {
                                MessageBox.Show("Please input field [ " + o.NAME + " ] attributed [ JOIN MODEL - JOIN FIELD ]");
                                return;
                            }
                            break;
                        case ControlKit.LOOKUP:
                            if (o.JOIN_TYPE == JoinType.JOIN_MODEL && (string.IsNullOrEmpty(o.JOIN_MODEL) || string.IsNullOrEmpty(o.JOIN_FIELD)))
                            {
                                MessageBox.Show("Please input field [ " + o.NAME + " ] attributed [ JOIN MODEL - JOIN FIELD ]");
                                return;
                            }
                            break;
                    }

                    if (o.JOIN_TYPE == JoinType.JOIN_MODEL && !string.IsNullOrEmpty(o.JOIN_MODEL) && !string.IsNullOrEmpty(o.JOIN_FIELD))
                    {
                        string[] types = db.GetFields(o.JOIN_MODEL).Where(x => x.NAME == o.JOIN_FIELD).Select(x => x.TYPE_NAME).ToArray();
                        if (types.Length > 0) o.TYPE_NAME = types[0];
                    }

                    if (o.JOIN_TYPE == JoinType.DEF_VALUE && o.VALUE_DEFAULT != null && o.VALUE_DEFAULT.Length >= 1 && o.VALUE_DEFAULT[0] != "")
                        o.TYPE_NAME = typeof(Int32).Name;

                    #endregion

                    if (o.FieldChange != dbFieldChange.REMOVE)
                        li.Add(o);
                }
                else
                {
                    MessageBox.Show("Please input fields: name, type, caption.");
                    c.Focus();
                    return;
                }
                index++;
            }//end for controls

            if (li.Count > 0)
            {
                DB_MODEL m = new DB_MODEL()
                {
                    NAME = dbName.Replace(" ", "_").Trim().ToUpper(),
                    FIELDS = li.ToArray(),
                };
                if (OnSubmit != null) OnSubmit(m);
            }
            else
            {
                MessageBox.Show("Please input fields: name, type, caption.");
            }
        }

    }
}
