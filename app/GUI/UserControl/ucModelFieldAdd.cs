using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using app.Core;
using System.Linq;
using System.Drawing;

namespace app.GUI
{

    public partial class ucModelFieldAdd : UserControl
    {
        private ControlKit[] Kits = Enum.GetValues(typeof(ControlKit)).OfType<ControlKit>().ToArray();
        private JoinType[] JoinTypes = Enum.GetValues(typeof(JoinType)).OfType<JoinType>().ToArray();


        const int hiBox = 150;

        const int _topLine1 = 5;
        const int _topLine2 = 29;
        const int _topLine3 = 68;
        const int _topLine4 = 110;
        const int _topLine5 = 150;


        #region [ === VARIABLE === ]

        private TextBoxCustom txt_Name;
        private ComboBox cbo_Type;
        private CheckBox chk_Auto;
        private ComboBox cbo_Kit;

        private ComboBox cbo_LinkType;
        private TextBoxCustom txt_ValueDefault;
        private ComboBox cbo_LinkModel;
        private ComboBox cbo_LinkField;
        private ComboBox cbo_LinkView;

        private CheckBox chk_Index;
        private CheckBox chk_Null;

        private Button btn_Ext;

        private ucTextBoxH txt_Caption;
        private ucTextBoxH txt_CaptionShort;
        private ucTextBoxH txt_Des;
        private CheckBox chk_MobiShow;
        private CheckBox chk_TabletShow;
        private CheckBox chk_Duplicate;
        private CheckBox chk_Encrypt;

        private CustomComboBox cbo_FuncValidate;
        private CustomComboBox cbo_FuncBeforeUpdate;
        private ucTextBoxH txt_OrderByView;
        private ucTextBoxH txt_OrderByEdit;

        private ucTextBoxH txt_KeyUri;

        private Button btn_Remove;
        private TextBox txt_FieldChange;

        private CheckBox chk_ShowInGrid;
        private CheckBox chk_IsFullTextSearch;
        private CheckBox chk_IsKeySync;

        #endregion

        public ucModelFieldAdd(int index, IDataFile db)
        {
            if (index != 0 && index % 2 == 0) BackColor = Color.Gray;

            string[] models = db.GetListDB();

            #region [ === UI === ]

            //////////////////////////////////////////////////////////////////////
            // LINE 1: 

            txt_Name = new TextBoxCustom() { Left = 4, Top = _topLine1, Width = 80, Name = "name" + index.ToString(), WaterMark = "Name ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            cbo_Type = new ComboBox() { Left = 88, Top = _topLine1, Width = 60, Name = "type" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            chk_Auto = new CheckBox() { Left = 164, Top = _topLine1, Width = 22, Name = "auto" + index.ToString() };
            for (int k = 0; k < dbType.Types.Length; k++) cbo_Type.Items.Add(dbType.Types[k]);
            cbo_Type.SelectedIndex = 0;
            cbo_Kit = new ComboBox() { Left = 192, Top = _topLine1, Width = 80, Name = "kit" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            foreach (ControlKit kit in Kits)
                cbo_Kit.Items.Add(new ComboboxItem() { Text = kit.ToString().ToUpper(), Value = ((int)kit) });
            cbo_Kit.SelectedIndex = 0;

            cbo_LinkType = new ComboBox() { Left = 276, Top = _topLine1, Width = 84, Name = "link_type" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (JoinType ti in JoinTypes)
                cbo_LinkType.Items.Add(new ComboboxItem() { Text = ti.ToString().ToUpper(), Value = ((int)ti) });
            cbo_LinkType.SelectedIndex = 0;
            txt_ValueDefault = new TextBoxCustom() { Left = 363, Top = _topLine1, Width = 307, Name = "value_default" + index.ToString(), WaterMark = "Default value: v1|v2|...", BorderStyle = BorderStyle.FixedSingle };
            cbo_LinkModel = new ComboBox() { Visible = false, Left = 363, Top = _topLine1, Width = 100, Name = "link_model" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            cbo_LinkField = new ComboBox() { Visible = false, Left = 465, Top = _topLine1, Width = 100, Name = "link_field" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            cbo_LinkModel.Items.Add(new ComboboxItem() { Text = "", Value = "" });
            for (int k = 0; k < models.Length; k++)
                cbo_LinkModel.Items.Add(new ComboboxItem() { Text = models[k].ToUpper(), Value = models[k] });
            cbo_LinkModel.SelectedIndex = 0;
            cbo_LinkView = new ComboBox() { Visible = false, Left = 568, Top = _topLine1, Width = 100, Name = "link_view" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };

            chk_Index = new CheckBox() { Left = 684, Top = _topLine1, Width = 20, Name = "index" + index.ToString(), Checked = false };
            chk_Null = new CheckBox() { Left = 720, Top = _topLine1, Width = 15, Name = "null" + index.ToString(), Checked = true };

            //////////////////////////////////////////////////////////////////////
            // LINE 2:


            txt_Caption = new ucTextBoxH(100) { Left = 4, Top = _topLine2,  Name = "caption" + index.ToString(), Title = "Caption" };
            txt_CaptionShort = new ucTextBoxH(100) { Left = 112, Top = _topLine2, Name = "caption_short" + index.ToString(), Title = "Caption short" };
            txt_Des = new ucTextBoxH(130) { Left = 216, Top = _topLine2,  Name = "des" + index.ToString(), Title = "Description" };

            chk_MobiShow = new CheckBox() { Left = 358, Top = _topLine1 + 30, Name = "mobi" + index.ToString(), Text = "Show Mobi", Width = 80, Checked = true };
            chk_TabletShow = new CheckBox() { Left = 440, Top = _topLine1 + 30, Name = "tablet" + index.ToString(), Text = "Show Tablet", Width = 90, Checked = true };
            chk_Duplicate = new CheckBox() { Left = 530, Top = _topLine1 + 30, Name = "duplicate" + index.ToString(), Text = "Duplicate", Width = 75, Checked = true };
            chk_Encrypt = new CheckBox() { Left = 607, Top = _topLine1 + 30, Name = "encrypt" + index.ToString(), Text = "Encrypt", Width = 66, Checked = false };

            txt_FieldChange = new TextBox() { Visible = false, Name = "field_change" + index.ToString(), Text = ((int)dbFieldChange.ADD).ToString() };
            btn_Ext = new Button() { Text = "+", Left = 744, Top = _topLine1, Width = 20, BackColor = SystemColors.Control };
            btn_Remove = new Button() { Text = "Remove", Left = 704, Top = _topLine1 + 30, Width = 60, BackColor = SystemColors.Control };
            btn_Remove.Click += (se, ev) => remove_Field(txt_FieldChange);
            btn_Ext.Click += (se, ev) =>
            {
                if (btn_Ext.Text == "-")
                {
                    btn_Ext.Text = "+";
                    this.Height = this.Height - hiBox;
                }
                else
                {
                    btn_Ext.Text = "-";
                    this.Height = this.Height + hiBox;
                }
            };

            //////////////////////////////////////////////////////////////////////
            // LINE 3:

            txt_OrderByEdit = new ucTextBoxH(100) { Left = 4, Top = _topLine3,  Name = "order_edit" + index.ToString(), Text = "99", Title = "Order on form", TextAlign = HorizontalAlignment.Center, OnlyInputNumber0To9 = true };
            txt_OrderByView = new ucTextBoxH(100) { Left = 112, Top = _topLine3,  Name = "order_view" + index.ToString(), Text = "99", Title = "Order on grid", TextAlign = HorizontalAlignment.Center, OnlyInputNumber0To9 = true };
            
            
            //txt_Des = new TextBoxCustom() { Left = 216, Top = _top + 30, Width = 130, Name = "des" + index.ToString(), WaterMark = "Description ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            //////////////////////////////////////////////////////////////////
            cbo_FuncValidate = new CustomComboBox()
            {
                Left = 358,
                Top = _topLine3,
                Width = 300,
                Name = "func_edit" + index.ToString(),
                Text = dbFunc.title_FUNC_VALIDATE_ON_FORM,
            };
            string[] afunc = dbFunc.GetFuncValidate();
            int index_VALIDATE_EMPTY = afunc.FindIndex(x => x == dbFunc.VALIDATE_EMPTY);
            CheckedListBox list_FuncValidate = new CheckedListBox() { BorderStyle = BorderStyle.None, Width = 300, };
            foreach (string fi in afunc) list_FuncValidate.Items.Add(new ComboboxItem() { Text = fi, Value = fi });
            if (index_VALIDATE_EMPTY != -1)
            {
                list_FuncValidate.SetItemChecked(index_VALIDATE_EMPTY, true);
                cbo_FuncValidate.Text = dbFunc.VALIDATE_EMPTY;
            }
            cbo_FuncValidate.KeyPress += (se, ev) => { ev.Handled = true; };
            cbo_FuncValidate.DropDownControl = list_FuncValidate;
            //cbo_FuncValidate.DropDownWidth = 200;
            cbo_FuncValidate.DropDown += (se, ev) => { };
            cbo_FuncValidate.DropDownClosed += (se, ev) => { };
            list_FuncValidate.ItemCheck += (se, ev) =>
            {
                List<string> li = new List<string>();
                foreach (var o in list_FuncValidate.CheckedItems)
                    li.Add(o.ToString());

                string it = afunc[ev.Index];
                CheckState val = ev.NewValue;
                if (val == CheckState.Checked)
                    li.Add(it);
                else
                    li.Remove(it);

                if (li.Count > 2)
                    cbo_FuncValidate.Text = "(" + li.Count.ToString() + ") Func validate on form";
                else
                    cbo_FuncValidate.Text = string.Join(",", li.Distinct().ToArray());
            };

            //////////////////////////////////////////////////////////////////////
            // LINE 4

            cbo_FuncBeforeUpdate = new CustomComboBox()
            {
                Left = 358,
                Top = _topLine4,
                Width = 300,
                Name = "func_before_update" + index.ToString(),
                Text = dbFunc.title_FUNC_BEFORE_ADD_OR_UPDATE,
            };
            string[] afuncUpdate = dbFunc.GetFuncBeforeAddOrUpdate();
            CheckedListBox list_FuncUpdate = new CheckedListBox() { BorderStyle = BorderStyle.None, Width = 300, };
            foreach (string fi in afuncUpdate) list_FuncUpdate.Items.Add(new ComboboxItem() { Text = fi, Value = fi });
            cbo_FuncBeforeUpdate.KeyPress += (se, ev) => { ev.Handled = true; };
            cbo_FuncBeforeUpdate.DropDownControl = list_FuncUpdate;
            //cbo_FuncBeforeUpdate.DropDownWidth = 200;
            cbo_FuncBeforeUpdate.DropDown += (se, ev) => { };
            cbo_FuncBeforeUpdate.DropDownClosed += (se, ev) => { };
            list_FuncUpdate.ItemCheck += (se, ev) =>
            {
                List<string> li = new List<string>();
                foreach (var o in list_FuncUpdate.CheckedItems)
                    li.Add(o.ToString());

                string it = afuncUpdate[ev.Index];
                CheckState val = ev.NewValue;
                if (val == CheckState.Checked)
                    li.Add(it);
                else
                    li.Remove(it);

                if (li.Count > 2)
                    list_FuncUpdate.Text = "(" + li.Count.ToString() + ") Func validate on form";
                else
                    cbo_FuncBeforeUpdate.Text = string.Join(",", li.Distinct().ToArray());
            };

            txt_KeyUri = new ucTextBoxH(100)
            {
                Left = 4,
                Top = _topLine4,
                Name = "key_url" + index.ToString(),
                Title = "Position key url",
                TextAlign = HorizontalAlignment.Center,
                OnlyInputNumber0To9 = true
            };

            //////////////////////////////////////////////////////////////////////
            // LINE 5

            chk_ShowInGrid = new CheckBox()
            {
                Text = "Show only query detail",
                Left = 4,
                Top = _topLine5,
                Name = "show_in_grid" + index.ToString(),
                Width = 150,
            };

            chk_IsFullTextSearch = new CheckBox()
            {
                Text = "Is full text search",
                Left = 160,
                Top = _topLine5,
                Name = "full_text_search" + index.ToString(),
                Width = 120,
            };

            chk_IsKeySync = new CheckBox()
            {
                Text = "Is key for sync or edit",
                Left = 320,
                Top = _topLine5,
                Name = "key_for_sync" + index.ToString(),
                Width = 150,
            };

            //////////////////////////////////////////////////////////////////
            this.Controls.AddRange(new Control[] { txt_Name, cbo_Type, chk_Auto, cbo_Kit,
                cbo_LinkType,txt_ValueDefault, cbo_LinkModel,cbo_LinkField,cbo_LinkView,
                chk_Index, chk_Null,
                btn_Ext,txt_Caption, txt_CaptionShort, txt_Des, chk_MobiShow, chk_TabletShow, chk_Duplicate, chk_Encrypt,
                txt_OrderByView, txt_OrderByEdit, cbo_FuncValidate,
                cbo_FuncBeforeUpdate, txt_KeyUri,
                btn_Remove,txt_FieldChange,
                chk_ShowInGrid,chk_IsFullTextSearch,chk_IsKeySync
            });

            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////////////

            #region [ === EVENT === ]

            cbo_LinkModel.Visible = false;
            cbo_LinkField.Visible = false;
            cbo_LinkView.Visible = false;
            int ijt = JoinTypes.FindIndex(x => x == JoinType.DEF_VALUE);
            cbo_LinkType.SelectedIndex = ijt;

            cbo_Type.SelectedIndexChanged += (se, ev) =>
            {
                string type = dbType.Types[cbo_Type.SelectedIndex];
                if (type == typeof(Boolean).Name)
                {
                    chk_Auto.Checked = false;
                    chk_Auto.Visible = false;
                    int iKit = Kits.FindIndex(x => x == ControlKit.CHECK);
                    cbo_Kit.SelectedIndex = iKit;
                    cbo_Kit.Enabled = false;
                    cbo_LinkType.Visible = false;
                    //return;
                }
                else
                {
                    chk_Auto.Visible = true;
                    cbo_Kit.Enabled = true;
                    cbo_Kit.SelectedIndex = 0;
                    cbo_LinkType.Visible = true;
                    //return;
                }

                if (type == typeof(DateTime).Name)
                {
                    int iKit = Kits.FindIndex(x => x == ControlKit.TEXT_DATETIME);
                    cbo_Kit.SelectedIndex = iKit;
                    cbo_Kit.Enabled = false;
                    //return;
                }
                else
                {
                    cbo_Kit.Enabled = true;
                    cbo_Kit.SelectedIndex = 0;
                    //return;
                }
            };

            cbo_Kit.SelectedIndexChanged += (se, ev) => kit_Change();

            cbo_LinkType.SelectedIndexChanged += (se, ev) =>
            {
                JoinType ji = JoinTypes[cbo_LinkType.SelectedIndex];
                switch (ji)
                {
                    case JoinType.NONE:
                        cbo_LinkType.SelectedIndex = 1;
                        break;
                    case JoinType.DEF_VALUE:
                        txt_ValueDefault.Visible = true;
                        cbo_LinkModel.Visible = false;
                        cbo_LinkField.Visible = false;
                        cbo_LinkView.Visible = false;
                        break;
                    case JoinType.JOIN_MODEL:
                        txt_ValueDefault.Visible = false;
                        cbo_LinkModel.Visible = true;
                        cbo_LinkField.Visible = true;
                        cbo_LinkView.Visible = true;
                        ControlKit kit = Kits[cbo_Kit.SelectedIndex];
                        if (kit != ControlKit.SELECT && kit != ControlKit.LOOKUP)
                        {
                            int _ix = Kits.FindIndex(x => x == ControlKit.SELECT);
                            if (_ix != -1) cbo_Kit.SelectedIndex = _ix;
                        }
                        break;
                }
            };
            cbo_LinkModel.SelectedIndexChanged += (se, ev) =>
            {
                cbo_LinkField.Items.Clear();
                cbo_LinkView.Items.Clear();

                if (cbo_LinkModel.SelectedIndex > 0)
                {
                    string m = models[cbo_LinkModel.SelectedIndex - 1];
                    if (!string.IsNullOrEmpty(m))
                    {
                        var fs = db.GetFields(m).ToArray();
                        if (fs.Length > 0)
                        {
                            for (int k = 0; k < fs.Length; k++)
                            {
                                cbo_LinkField.Items.Add(new ComboboxItem() { Text = fs[k].NAME.ToUpper() + " - " + fs[k].Type.Name, Value = fs[k].NAME });
                                cbo_LinkView.Items.Add(new ComboboxItem() { Text = fs[k].NAME.ToUpper() + " - " + fs[k].Type.Name, Value = fs[k].NAME });
                            }
                            cbo_LinkField.SelectedIndex = 0;
                            cbo_LinkView.SelectedIndex = 0;
                        }
                    }
                }
            };

            chk_Auto.CheckedChanged += (se, ev) =>
            {
                kit_Change();
                if (chk_Auto.Checked)
                {
                    // FIELD KEY AUTO
                    chk_Null.Checked = false;
                    chk_Null.Visible = false;
                    cbo_Kit.Visible = false;
                    cbo_LinkType.Visible = false;
                    txt_ValueDefault.Visible = false;
                    cbo_LinkModel.Visible = false;
                    cbo_LinkField.Visible = false;
                    cbo_LinkView.Visible = false;
                }
                else
                {
                    // FIELD DATA
                    chk_Null.Visible = true;
                    cbo_Kit.Visible = true;
                    cbo_LinkType.Visible = true;
                    txt_ValueDefault.Visible = true;
                }
            };

            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////////////
        }

        private void kit_Change()
        {
            ControlKit kit = Kits[cbo_Kit.SelectedIndex];
            switch (kit)
            {
                case ControlKit.SELECT:
                    cbo_LinkType.Visible = true;
                    cbo_LinkType.Enabled = true;
                    JoinType ji = JoinTypes[cbo_LinkType.SelectedIndex];
                    switch (ji)
                    {
                        case JoinType.NONE:
                            cbo_LinkType.SelectedIndex = 1;
                            break;
                        case JoinType.JOIN_MODEL:
                            txt_ValueDefault.Visible = false;
                            cbo_LinkModel.Visible = true;
                            cbo_LinkField.Visible = true;
                            cbo_LinkView.Visible = true;
                            break;
                        default:
                            txt_ValueDefault.Visible = true;
                            cbo_LinkModel.Visible = false;
                            cbo_LinkField.Visible = false;
                            cbo_LinkView.Visible = false;
                            break;
                    }
                    break;
                case ControlKit.LOOKUP:
                    cbo_LinkType.Enabled = false;
                    cbo_LinkType.Visible = true;
                    cbo_LinkType.SelectedIndex = 2;
                    txt_ValueDefault.Visible = false;
                    cbo_LinkModel.Visible = true;
                    cbo_LinkField.Visible = true;
                    cbo_LinkView.Visible = true;
                    break;
                default:
                    if (kit == ControlKit.TEXT_PASS)
                        chk_Encrypt.Checked = true;
                    else
                        chk_Encrypt.Checked = false;

                    cbo_LinkType.Visible = true;
                    cbo_LinkType.Enabled = true;
                    txt_ValueDefault.Visible = true;
                    cbo_LinkModel.Visible = false;
                    cbo_LinkField.Visible = false;
                    cbo_LinkView.Visible = false;
                    break;
            }
        }

        private void remove_Field(TextBox txt_FieldChange)
        {
            if (MessageBox.Show("Are you sure remove this fields ?", "Remove Field",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                txt_FieldChange.Text = ((int)dbFieldChange.REMOVE).ToString();
                this.Visible = false;
            }
        }

    }

}
