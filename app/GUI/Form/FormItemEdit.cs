using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System;

namespace app.GUI
{
    public class FormItemEdit : FormBase, IDataFieldEvent
    {
        const int fWidth = 800;
        const int fHeight = 550;

        public delegate void EventSubmit(string dbName, Dictionary<string, object> data);
        public EventSubmit OnSubmit;

        private readonly IDataFile db;
        private readonly FlowLayoutPanel boi_Filter;
        private readonly object ItemEdit;
        
        public FormItemEdit(IDataFile _db, string dbName, object _itemEdit)
            : base("EDIT ITEM SELECTED", true)
        {
            ItemEdit = _itemEdit;
            int _Hi = 0;
            db = _db;
            ClientSize = new Size(fWidth, Screen.PrimaryScreen.WorkingArea.Height - 80);
            Top = 40;

            ////////////////////////////////////////////////////////////////////////////////

            var model = db.GetModel(dbName);

            #region [ === CONTROLS UI === ]

            boi_Filter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(0),
                BackColor = Color.WhiteSmoke,
                FlowDirection = FlowDirection.LeftToRight,
            };
            boi_Filter.MouseDown += FormMove_MouseDown;

            var fields = model.FIELDS.Where(x => x.IS_KEY_AUTO == false).OrderBy(x => x.ORDER_EDIT).ToArray();
            Type typeItem = ItemEdit.GetType();
            for (int ki = 0; ki < fields.Length; ki++)
            {
                var fd = fields[ki];
                var po = typeItem.GetProperty(fd.NAME);
                if (po != null) fd.Value = po.GetValue(ItemEdit, null);
                var uc = new ucDataItemEdit(dbName, ki, fd, _db, this) { Name = "uc" + ki.ToString(), };
                uc.Height = uc._Height;
                uc.Width = uc._Width;
                boi_Filter.Controls.Add(uc);
            }

            int kitMin = ((fields.Select(x => x.KIT).Where(x => x != ControlKit.HTML && x != ControlKit.TEXTAREA)
                .Count() / 2) + 1);
            int hi_Min = (kitMin * ucDataItemAdd.Height_Min) + (kitMin * 10);
            int hi_Max = fields.Select(x => x.KIT).Where(x => x == ControlKit.TEXTAREA)
                .Count() * ucDataItemAdd.Height_Max;
            int hi_Full = fields.Select(x => x.KIT).Where(x => x == ControlKit.HTML)
                .Count() * ucDataItemAdd.Height_Full;
            _Hi = hi_Min + hi_Max + hi_Full + 60;

            Panel boi_Action = new Panel() { Dock = DockStyle.Bottom, Height = 25 };
            Button btn_DataTest = new Button() { Dock = DockStyle.Right, Text = "FILL DATA TEST", BackColor = Color.WhiteSmoke, Width = 123, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Reset = new Button() { Dock = DockStyle.Right, Text = "RESET", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            boi_Action.Controls.AddRange(new Control[] { btn_DataTest, btn_Submit, btn_Reset });
            boi_Action.MouseDown += FormMove_MouseDown;

            this.Controls.AddRange(new Control[] { boi_Filter, boi_Action });
            boi_Action.BringToFront();
            boi_Filter.BringToFront();
            btn_Submit.Focus();

            #endregion

            if (_Hi < Screen.PrimaryScreen.WorkingArea.Height)
            {
                if (_Hi < 200) _Hi = 200;
                ClientSize = new System.Drawing.Size(fWidth, _Hi);
            }

            btn_Submit.Click += (se, ev) => form_Submit(dbName);
            btn_DataTest.Click += (se, ev) => fill_DataTest();

            HideScrollBar(boi_Filter.Handle, ScrollBarHide.SB_HORZ);

            ////////////////////////////////////////////////////////////////////////////////
            
        }

        private readonly Type m_TypeField = typeof(FieldInfo);
        private void form_Submit(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                MessageBox.Show("Please input Model Name and fields: name, type, caption.");
                return;
            }

            var dt = new Dictionary<string, object>();
            foreach (Control c in boi_Filter.Controls)
            {
                foreach (Control fi in c.Controls)
                {
                    if (fi.Tag != null && fi.Tag.GetType() == m_TypeField)
                    {
                        object val = null;
                        IFieldInfo fo = (IFieldInfo)fi.Tag;
                        
                        #region [=== ===]

                        switch (fo.KIT)
                        {
                            case ControlKit.LABEL: // Label 
                                val = fi.Text;
                                break;
                            case ControlKit.CHECK: // CheckBox 
                                val = (fi as CheckBox).Checked;
                                break;
                            case ControlKit.RADIO: // RadioButton 
                                break;
                            case ControlKit.COLOR: // Label 
                                val = fi.Text;
                                break;
                            case ControlKit.SELECT: // ComboBox 
                                val = (fi as ComboBox).SelectedValue;
                                break;
                            case ControlKit.TEXT_PASS: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXT_DATE:
                            case ControlKit.TEXT_DATETIME:
                            case ControlKit.TEXT_TIME: // DateTimePicker 
                                val = (fi as DateTimePicker).Value;
                                break;
                            case ControlKit.TEXT_EMAIL: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXT_FILE: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXTAREA:
                            case ControlKit.HTML: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.LOOKUP: // TextBox  
                                val = fi.Text;
                                break;
                            default:  // TextBox   
                                val = fi.Text;
                                break;
                        }
                        
                        #endregion

                        if (val == null || (fo.TYPE_NAME == typeof(String).Name && val.ToString() == ""))
                        {
                            if (fo.FUNC_EDIT.Contains(dbFunc.VALIDATE_EMPTY))
                            {
                                MessageBox.Show("Please input data for field [" + fo.NAME + "]", dbFunc.VALIDATE_EMPTY);
                                return;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(fo.FUNC_BEFORE_UPDATE))
                            {
                                //val = dbFunc.ExeFuncBeforeAddOrUpdate(fo.FuncBeforeUpdate, val);
                            }

                            dt.Add(fo.NAME, val);
                        }
                    }
                }//end for fields

            }//end for controls
            if (dt.Count > 0)
            {
                if (OnSubmit != null) OnSubmit(dbName, dt);
                else
                {
                    MessageBox.Show("Please input fields.");
                }
            }
        }

        private void fill_DataTest()
        {
            foreach (Control c in boi_Filter.Controls)
            {
                foreach (Control fi in c.Controls)
                {
                    if (fi.Tag != null && fi.Tag.GetType() == typeof(FieldInfo))
                    {
                        FieldInfo field = (FieldInfo)fi.Tag;
                        switch (field.KIT)
                        {
                            case ControlKit.TEXT_DATE:
                            case ControlKit.TEXT_DATETIME:
                            case ControlKit.TEXT_TIME:
                                break;
                            case ControlKit.TEXT:
                            case ControlKit.TEXTAREA:
                            case ControlKit.TEXT_EMAIL:
                            case ControlKit.TEXT_FILE:
                            case ControlKit.TEXT_PASS:
                                fi.Text = dbType.CreateValueRandom(field.TYPE_NAME).ToString();
                                break;
                            case ControlKit.CHECK:
                                break;
                            case ControlKit.COLOR:
                                break;
                            case ControlKit.HTML:
                                fi.Text = dbType.CreateValueRandom(field.TYPE_NAME).ToString();
                                break;
                            case ControlKit.LABEL:
                                fi.Text = dbType.CreateValueRandom(field.TYPE_NAME).ToString();
                                break;
                            case ControlKit.LOOKUP:
                                break;
                            case ControlKit.RADIO:
                                break;
                            case ControlKit.SELECT:
                                var co = (fi as ComboBox);
                                if (co.Items.Count > 0)
                                    co.SelectedIndex = new Random().Next(0, co.Items.Count - 1);
                                break;
                        }
                    }
                }
            }
        }

        public void BindDataUC(IDictionary<string, object> data)
        {
            foreach (Control c in boi_Filter.Controls)
            {
                foreach (Control fi in c.Controls)
                {
                    if (fi.Tag != null && fi.Tag.GetType() == typeof(FieldInfo))
                    {
                        FieldInfo field = (FieldInfo)fi.Tag;

                        if (!data.ContainsKey(field.NAME)) continue;
                        object val = data[field.NAME];

                        switch (field.KIT)
                        {
                            case ControlKit.TEXT_DATE:
                            case ControlKit.TEXT_DATETIME:
                            case ControlKit.TEXT_TIME:
                                break;
                            case ControlKit.TEXT:
                            case ControlKit.TEXTAREA:
                            case ControlKit.TEXT_EMAIL:
                            case ControlKit.TEXT_FILE:
                            case ControlKit.TEXT_PASS:
                                fi.Text = val as string;
                                break;
                            case ControlKit.CHECK:
                                break;
                            case ControlKit.COLOR:
                                break;
                            case ControlKit.HTML:
                                break;
                            case ControlKit.LABEL:
                                fi.Text = val as string;
                                break;
                            case ControlKit.LOOKUP:
                                break;
                            case ControlKit.RADIO:
                                break;
                            case ControlKit.SELECT:
                                fi.Text = val as string;
                                break;
                        }
                    }
                }
            }

        }//end function
    }
}
