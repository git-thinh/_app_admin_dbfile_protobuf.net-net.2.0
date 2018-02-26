using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using app.Core;
using System.Linq;
using System.Linq.Dynamic;
using Deveck.Ui.Controls;
using Deveck.Ui.Controls.Scrollbar;
using System.Collections;
using System.Threading;
using app.Model;

namespace app.GUI
{
    public class FormDB : Form
    {
        #region [ === VARIABLE === ]

        public delegate void EventExit();
        public EventExit OnExit;

        private readonly IDataFile db;

        private ToolStripLabel lbl_User;
        private ToolStripLabel lbl_Title;
        private TabControlCustom mui_tabModel;

        private readonly Dictionary<string, int> m_tabIndex;
        private readonly TreeView mui_modelTreeView;
        private readonly TextBox mui_modelSearchTextBox;

        private string m_modelCurrent = "";

        #endregion

        public FormDB(IDataFile _db)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Font = App.Font;
            this.Text = App.Name;

            m_tabIndex = new Dictionary<string, int>();
            mui_modelTreeView = new TreeView() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None };
            mui_modelSearchTextBox = new TextBox() { Dock = DockStyle.Top, BackColor = App.ColorControl, Width = 100, Height = 20, Margin = new Padding(10, 10, 0, 0), Text = "", BorderStyle = BorderStyle.FixedSingle };

            db = _db;
            init_Control();
        }


        #region [ === FORM MOVE === ]

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

        public void ShowUser(string user) 
        {
            lbl_User.Text = "[" + user + "]";
        }

        private void init_Control()
        {
            this.Padding = App.FormBorder;
            this.BackColor = App.ColorBorder;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(App.Width, 555);

            ////////////////////////////////////////////////////

            #region [ === MENU === ]

            MenuStrip menu = new MenuStrip() { Dock = DockStyle.Top, BackColor = App.ColorBg };
            //ToolStripTextBox mn_DbSearch = new ToolStripTextBox() { Width = left_Width, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray };
            lbl_User = new ToolStripLabel() { Text = "[" + App.UserCurrent + "]", ForeColor = Color.White, };
            lbl_Title = new ToolStripLabel() { Text = "// ... ", ForeColor = Color.White, };
            lbl_Title.Click += (se, ev) => listDB_BindData();
            ToolStripMenuItem mn = new ToolStripMenuItem() { Text = "MODEL", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right, };
            var mn_ModelAdd = new ToolStripMenuItem() { Text = "Model Add" };
            mn_ModelAdd.Click += (se, ev) => model_Add();
            var mn_ModelEdit = new ToolStripMenuItem() { Text = "Model Edit" };
            mn_ModelEdit.Click += (se, ev) => model_Edit();
            var mn_ModelRemove = new ToolStripMenuItem() { Text = "Model Remove" };
            mn_ModelRemove.Click += (se, ev) => model_Remove();
            var mn_ModelTruncate = new ToolStripMenuItem() { Text = "Model Truncate" };
            mn_ModelTruncate.Click += (se, ev) => model_Truncate();
            var mn_ItemAdd = new ToolStripMenuItem() { Text = "Item Add" };
            mn_ItemAdd.Click += (se, ev) => item_Add();
            var mn_ItemEdit = new ToolStripMenuItem() { Text = "Item Edit" };
            mn_ItemEdit.Click += (se, ev) => item_Edit();
            var mn_ItemRemove = new ToolStripMenuItem() { Text = "Item Remove" };
            mn_ItemRemove.Click += (se, ev) => item_Remove();
            mn.DropDownItems.AddRange(new ToolStripItem[] { mn_ModelAdd, mn_ModelEdit, mn_ModelRemove, mn_ModelTruncate, mn_ItemAdd, mn_ItemEdit, mn_ItemRemove });
            ToolStripItem mn_Hide = new ToolStripMenuItem() { Text = "HIDE", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right };
            ToolStripItem mn_Exit = new ToolStripMenuItem() { Text = "EXIT", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right, };
            mn_Hide.Click += (se, ev) => { this.Hide(); };
            mn_Exit.Click += (se, ev) => { if (OnExit != null) OnExit(); };
            menu.Items.AddRange(new ToolStripItem[] { 
                lbl_User,
                lbl_Title, mn_Exit, mn_Hide, mn, });
            menu.MouseDown += Form_MouseDown;
            this.Controls.Add(menu);

            #endregion

            ////////////////////////////////////////////////////

            #region [ === FORM - BOX LEFT === ]

            Panel box_Left = new Panel() { Dock = DockStyle.Left, Width = App.col_Left_Width, BackColor = Color.White, Margin = new Padding(0), Padding = new Padding(0), };

            box_Left.Controls.AddRange(new Control[] {
               // mui_modelSearchTextBox,
                mui_modelTreeView, 
            });

            this.Controls.Add(box_Left);

            #endregion

            ////////////////////////////////////////////////////

            #region [ === FORM - BOX RIGHT === ]

            Splitter splitter = new Splitter() { Dock = DockStyle.Left, BackColor = App.ColorBg, Width = App.col_Splitter_Width, MinSize = 0 };

            Panel box_Right = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, };
            mui_tabModel = new TabControlCustom() { Dock = DockStyle.Fill };

            ////Panel box_Footer = new Panel() { Dock = DockStyle.Bottom, Height = 20, Padding = new Padding(1, 1, 1, 0), BackColor = App.ColorBg };
            ////box_Footer.MouseDown += Label_MouseDown;

            ////ButtonCustom btn_ModelAdd = new ButtonCustom() { Text = "Tab Add", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ////ButtonCustom btn_ModelEdit = new ButtonCustom() { Text = "Tab Edit", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ////ButtonCustom btn_ModelRemove = new ButtonCustom() { Text = "Tab Remove", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };

            ////ButtonCustom btn_ItemAdd = new ButtonCustom() { Text = "Item Add", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ////ButtonCustom btn_ItemEdit = new ButtonCustom() { Text = "Item Edit", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ////ButtonCustom btn_ItemRemove = new ButtonCustom() { Text = "Item Remove", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };

            ////box_Footer.Controls.AddRange(new Control[] {
            ////    btn_ItemRemove,
            ////    btn_ItemEdit,
            ////    btn_ItemAdd,

            ////    btn_ModelRemove,
            ////    btn_ModelEdit,
            ////    btn_ModelAdd,
            ////});

            box_Right.Controls.AddRange(new Control[] {
                mui_tabModel,
                //box_Footer,
            });

            #endregion

            ////////////////////////////////////////////////////
            // LOAD CONTROL

            this.Controls.Add(box_Left);
            this.Controls.Add(splitter);
            this.Controls.Add(box_Right);
            box_Left.BringToFront();
            mui_modelTreeView.BringToFront();
            splitter.BringToFront();
            box_Right.BringToFront();

            ////////////////////////////////////////////////////

            mui_modelTreeView_BindData();
        }

        #region [ === TREEVIEW === ]

        void mui_modelTreeView_BindData()
        {
            foreach (string tag in db.menu_GetTAG())
            {
                TreeNode node = new TreeNode() { Text = tag };
                foreach (var m in db.menu_Find(x => x.TAG == tag))
                {
                    if (m_modelCurrent == "")
                    {
                        lbl_Title.Text = "// " + m.TITLE;
                        m_modelCurrent = m.MODELS;
                        model_View(m_modelCurrent);
                    }
                    node.Nodes.Add(new TreeNode() { Text = m.TITLE, Tag = m.MODELS });
                }
                mui_modelTreeView.Nodes.Add(node);
            }
            mui_modelTreeView.DoubleClick += (se, ev) => mui_modelTreeView_Click();
            mui_modelTreeView.BeforeExpand += (se, ev) => { mui_modelTreeView.CollapseAll(); };
        }

        void mui_modelTreeView_Click()
        {
            TreeNode node = mui_modelTreeView.SelectedNode;
            if (node != null && node.Tag != null)
            {
                lbl_Title.Text = "// " + mui_modelTreeView.SelectedNode.Text;
                m_modelCurrent = node.Tag.ToString();
                model_View(m_modelCurrent);
            }
        }

        #endregion

        void model_View(string model)
        {
            int tabIndex = -1;
            if (m_tabIndex.TryGetValue(model, out tabIndex) && tabIndex != -1)
            {
                mui_tabModel.SelectedIndex = tabIndex;
                mui_tabModel.Tag = model;
            }
            else
            {
                mui_tabModel_View(model);
                tabIndex = mui_tabModel.TabCount - 1;
                m_tabIndex.Add(model, tabIndex);
                mui_tabModel.SelectedIndex = tabIndex;
                mui_modelTreeView.Focus();
            }
        }

        #region [ === LIST DB === ]

        private void listDB_BindData()
        {
            //listDB.Items.Clear();
            //dbName = db.GetListDB();

            //if (dbName.Length > 0)
            //    for (int k = tab.TabPages.Count; k < dbName.Length; k++) tabPage_CreateUI(dbName[k]);

            //for (int x = 0; x < dbName.Length; x++)
            //    listDB.Items.Add(dbName[x]);
            ////for (int x = dbName.Length; x < 999; x++) listDB.Items.Add("Auto test " + x.ToString()); 

            //if (dbName.Length > 0)
            //{
            //    int _index = 0;
            //    string name_cache = listDB.Tag as string;
            //    if (listDB.Tag != null) _index = dbName.FindIndex(x => x == name_cache);
            //    if (_index == -1 || _index >= dbName.Length) _index = 0;

            //    listDB.Items[_index].Selected = true;
            //    if (_index < tab.TabPages.Count)
            //        tab.SelectedIndex = _index;
            //}
        }

        private void listDB_SelectedIndexChanged()
        {
            //int tabIndex = -1;
            //if (listDB.SelectedItems.Count > 0)
            //    tabIndex = listDB.Items.IndexOf(listDB.SelectedItems[0]);
            //else return;

            ////tabIndex = list_DB.SelectedIndex;
            //if (tabIndex >= dbName.Length)
            //{
            //    lbl_Title.Text = "// ";
            //    tab.Visible = false;
            //    return;
            //}

            //string dbNameCurrent = dbName[tabIndex];
            //lbl_Title.Text = "// " + dbNameCurrent.ToUpper();

            //tab.SelectedIndex = tabIndex;
            //tab.Tag = dbNameCurrent;
        }

        #endregion

        #region [ === MODEL: ADD - EDIT - REMOVE - TRUNCATE === ]

        private void model_Add()
        {
            var fm = new FormModelAdd(db);
            fm.OnSubmit += (DB_MODEL m) =>
            {
                bool ok = db.CreateDb(m);
                if (ok)
                {
                    ////listDB.Items.Add(m.NAME);
                    ////dbName = db.GetListDB();
                    ////tabPage_CreateUI(m.NAME);
                    ////MessageBox.Show("Create model: " + m.NAME + " successfully.");
                    ////fm.Close();
                }
                else
                    MessageBox.Show("Create model: " + m.NAME + " fail.");
            };
            App.show_FormDialog(fm);
        }

        private void model_Edit()
        {
            var fm = new FormModelEdit(db, m_modelCurrent);
            fm.OnSubmit += (DB_MODEL m, bool hasRemoveField) =>
            {
                //int kAdd = m.Fields.Where(x => x.FieldChange == dbFieldChange.ADD).Count();
                //int kRemove = m.Fields.Where(x => x.FieldChange == dbFieldChange.REMOVE).Count();
                //if (kAdd > 0 && kRemove > 0)
                //{
                //    MessageBox.Show("Update model [" + m.Name + "] fail. It only allow execute ADD or REMOVE once time.", "Update Model");
                //    fm.Reset();
                //    return;
                //}

                bool ok = db.ModelUpdate(m, hasRemoveField);
                if (ok)
                {
                    MessageBox.Show("Update model: " + m.NAME + " successfully.", "Update Model");
                    //listDB.Items.Add(m.Name);
                    //dbName = db.GetListDB();
                    mui_tabModel_View(m.NAME, true);
                    //MessageBox.Show("Create model: " + m.Name + " successfully.");
                    fm.Close();
                }
                else
                    MessageBox.Show("Update model: " + m.NAME + " fail.", "Update Model");
            };
            App.show_FormDialog(fm);
        }

        private void model_Remove() { }

        private void model_Truncate()
        { 
            if (MessageBox.Show("Are you sure truncate model [" + m_modelCurrent + "] ?", "Truncate Model",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            EditStatus rs = db.ModelTruncate(m_modelCurrent);
            if (rs == EditStatus.SUCCESS)
                MessageBox.Show("Truncate model [" + m_modelCurrent + "] successfully", "Truncate Model");
            else
                MessageBox.Show("Truncate model [" + m_modelCurrent + "] fail", "Truncate Model");
        }

        #endregion

        #region [ === ITEM: ADD - EDIT - REMOVE === ]

        private void item_Add()
        { 
            var fm = new FormItemAdd(db, m_modelCurrent);
            fm.OnSubmit += (model, data) =>
            {
                object ok = db.AddItem(model, data);
                if (ok != null)
                {
                    MessageBox.Show("Add item model: " + model + " successfully.");
                    fm.Close();
                    (mui_tabModel.SelectedTab as TabPageCustom).OnLoadData();
                }
                else
                    MessageBox.Show("Add item model: " + model + " fail: " + ok.ToString());
            };
            fm.ShowDialog();
        }

        private void item_Edit()
        {
            if (GridItemSelected == null)
            {
                MessageBox.Show("Select item to edit");
                return;
            }

            var exist = db.ExistItemDynamic(m_modelCurrent, GridItemSelected);
            if (exist)
            {
                var fe = new FormItemEdit(db, m_modelCurrent, GridItemSelected);
                fe.ShowDialog();
            }
            else
                MessageBox.Show("Can not find item to edit");
        }

        private void item_Remove()
        {
            if (GridItemSelected == null)
            {
                MessageBox.Show("Select item to remove");
                return;
            } 
            if (MessageBox.Show("You will remove item selected ?", "Are you sure?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                var exist = db.ExistItemDynamic(m_modelCurrent, GridItemSelected);
                if (exist)
                {
                    bool val = db.RemoveItemDynamic(m_modelCurrent, GridItemSelected);
                    if (val)
                    {
                        MessageBox.Show("Remove item successfully");
                        (mui_tabModel.SelectedTab as TabPageCustom).OnLoadData();
                    }
                    else
                        MessageBox.Show("Remove item fail");
                }
                else
                    MessageBox.Show("Can not find item to remove");
            }
        }

        #endregion

        private object GridItemSelected = null;
        private const int selectTop = 100;
        private string[] OpString = "Equals,NotEquals,Contains,StartsWith,EndsWith".Split(',');
        private string[] OpNumber = "Equals,NotEquals,Contains,GreaterThan,LessThan,GreaterThanOrEqual,LessThanOrEqual".Split(',');
        private void mui_tabModel_View(string modelName, bool replacePage = false)
        {
            var info = db.GetInfoSelectTop(modelName, selectTop);
            if (info == null)
            {
                mui_tabModel.TabPages.Add(new TabPageCustom());
            }
            else
            {
                TabPageCustom page;
                if (replacePage)
                {
                    int tabIndex = -1;
                    if (m_tabIndex.TryGetValue(m_modelCurrent, out tabIndex) && tabIndex != -1)
                    {
                        page = (TabPageCustom)mui_tabModel.TabPages[tabIndex];
                        page.UpdateFields(info.Fields);
                        page.OnLoadData();
                    }
                }
                else
                {
                    page = new TabPageCustom(info.Fields) { BackColor = Color.White, };

                    #region [ === BOX FILTER === ]

                    int hi = (((info.Fields.Length / 3) + (info.Fields.Length % 3 == 0 ? 0 : 1) + 0) * 27);
                    if (info.Fields.Length < 4) hi = 27;
                    FlowLayoutPanel boi_Filter = new FlowLayoutPanel() { Visible = false, Dock = DockStyle.Top, Height = hi, AutoScroll = false, Padding = new Padding(0), BackColor = App.ColorBg };
                    boi_Filter.FlowDirection = FlowDirection.LeftToRight;
                    boi_Filter.MouseDown += Form_MouseDown;

                    for (int k = 1; k <= info.Fields.Length; k++)
                    {
                        var dp = info.Fields[k - 1];

                        Label lbl = new Label() { Name = "lbl" + k.ToString(), Text = dp.NAME, AutoSize = false, Width = 118, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight };
                        ComboBox cbo = new ComboBox() { Name = "cbo" + k.ToString(), Width = 80, BackColor = App.ColorControl, DropDownStyle = ComboBoxStyle.DropDownList };
                        if (dp.Type.Name == "String")
                            for (int ki = 0; ki < OpString.Length; ki++) cbo.Items.Add(OpString[ki]);
                        else
                            for (int ki = 0; ki < OpNumber.Length; ki++) cbo.Items.Add(OpNumber[ki]);
                        TextBox txt = new TextBox() { Name = "txt" + k.ToString(), Width = 80, BackColor = App.ColorControl, BorderStyle = BorderStyle.FixedSingle };
                        boi_Filter.Controls.AddRange(new Control[] {
                            lbl,
                            cbo,
                            txt,
                        });

                        if (k != 0 && k % 3 == 0)
                        {
                            Label sp = new Label() { Text = "", AutoSize = false, Width = App.Width - App.col_Left_Width, Height = 1, };
                            boi_Filter.Controls.Add(sp);
                        }
                    }//end for fields

                    #endregion

                    #region [ === BOX SEARCH - SHOW | HIDE FILTER === ]

                    Panel boi_Action = new Panel() { Dock = DockStyle.Top, Height = 27, AutoScroll = false, Padding = new Padding(0, 5, 10, 3), BackColor = App.ColorBg };
                    boi_Action.MouseDown += Form_MouseDown;
                    TextBox txt_Keyword = new TextBox() { Dock = DockStyle.Right, Width = 166, BorderStyle = BorderStyle.FixedSingle, BackColor = App.ColorControl };
                    ButtonCustom btn_Search = new ButtonCustom() { Text = "Search", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 60, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                    ButtonCustom btn_Filter = new ButtonCustom() { Text = "Filter", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 50, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                    ButtonCustom btn_ItemAdd = new ButtonCustom() { Text = "+", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 20, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                    btn_ItemAdd.Click += (se, ev) => item_Add();
                    boi_Action.Controls.AddRange(new Control[] {
                        txt_Keyword,
                        new Label() { Dock = DockStyle.Right, AutoSize = false, Width = 2, Height = 20, BackColor = App.ColorBg },
                        btn_Search,
                        new Label() { Dock = DockStyle.Right, AutoSize = false, Width = 2, Height = 20, BackColor = App.ColorBg },
                        btn_Filter,
                        new Label() { Dock = DockStyle.Right, AutoSize = false, Width = 2, Height = 20, BackColor = App.ColorBg },
                        btn_ItemAdd,
                    });

                    #endregion

                    #region [ === GRID === ]

                    var grid = new ListViewModelItem() { Dock = DockStyle.Fill };
                    grid.SetDataBinding(info.Fields, info.DataSelectTop);
                    grid.OnItemClick += (index, values) => { GridItemSelected = values; };

                    #endregion

                    #region [ === BOX FOOTER === ]

                    Panel boi_Footer = new Panel() { Dock = DockStyle.Bottom, Height = 18, AutoScroll = false, Padding = new Padding(0), BackColor = App.ColorBg };
                    boi_Footer.MouseDown += Form_MouseDown;

                    Label lbl_TotalRecord = new Label() { Dock = DockStyle.Left, Text = "Records: " + info.DataSelectTop.Count.ToString() + " / " + info.TotalRecord.ToString() + " ", AutoSize = false, Width = 199, Height = 18, Padding = new Padding(0), Font = new Font(new FontFamily("Arial"), 7F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.MiddleLeft };
                    Label lbl_Port = new Label() { Dock = DockStyle.Left, Text = "Port HTTP: " + info.PortHTTP.ToString(), AutoSize = false, Width = 110, Height = 18, Padding = new Padding(4, 0, 0, 0), Font = new Font(new FontFamily("Arial"), 7F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.MiddleLeft };
                    //ButtonCustom btn_Search = new ButtonCustom() { Text = "search", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                    ButtonCustom btn_PagePrev = new ButtonCustom() { Text = "<<<", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                    ButtonCustom btn_PageNext = new ButtonCustom() { Text = ">>>", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                    Label spa = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                    Label lbl_PageCurrent = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                    Label lbl_PageSP = new Label() { Text = " | ", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                    Label lbl_PageTotal = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                    lbl_PageTotal.Text = ((int)(info.TotalRecord / selectTop) + (info.TotalRecord % selectTop == 0 ? 0 : 1)).ToString();

                    boi_Footer.Controls.AddRange(new Control[] {
                        lbl_TotalRecord,
                        lbl_Port,

                        btn_PagePrev,
                        lbl_PageCurrent,
                        lbl_PageSP,
                        lbl_PageTotal,
                        btn_PageNext,
                    });

                    #endregion

                    page.Controls.AddRange(new Control[] { boi_Filter, boi_Action, grid, boi_Footer, });
                    mui_tabModel.TabPages.Add(page);

                    boi_Filter.BringToFront();
                    boi_Action.BringToFront();
                    grid.BringToFront();

                    btn_Filter.Click += (se, ev) => { if (boi_Filter.Visible) boi_Filter.Visible = false; else boi_Filter.Visible = true; };
                    ////////////////////////////////////////////////// 
                    btn_PageNext.Click += (se, ev) => tabPage_Next(modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                    btn_PagePrev.Click += (se, ev) => tabPage_Prev(modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                    ////////////////////////////////////////////////// 
                    txt_Keyword.KeyDown += (se, ev) =>
                    {
                        if (ev.KeyCode == Keys.Enter) tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                    };
                    btn_Search.Click += (se, ev) => tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                    page.OnLoadData += () =>
                    {
                        info = db.GetInfoSelectTop(modelName, selectTop);
                        tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                    };
                }
            }//end bind info Model
        }

        #region [ === FUNCTION: SEARCH - PREVIEW - NEXT === ]

        private void tabPage_Search(
            string kw, string modelName, InfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            //grid.DataSource = null;

            string predicate = "";
            if (string.IsNullOrEmpty(kw))
            {
                page.SearchRequest = null;
                page.SearchResult = null;

                page.PageCurrent = 1;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                int countPage = (int)(info.TotalRecord / selectTop) + (info.TotalRecord % selectTop == 0 ? 0 : 1);
                lbl_PageTotal.Text = countPage.ToString();
                lbl_TotalRecord.Text = "Records: " + info.DataSelectTop.Count.ToString() + " / " + info.TotalRecord.ToString() + " ";
                grid.SetDataBinding(info.Fields, info.DataSelectTop);
            }
            else
            {
                StringBuilder wh_Contain = new StringBuilder();
                for (int k = 0; k < info.Fields.Length; k++)
                {
                    var dp = info.Fields[k];
                    wh_Contain.Append(dp.NAME + (dp.Type.Name == "String" ? string.Empty : ".ToString()") + ".Contains(@0) ");
                    if (k < info.Fields.Length - 1) wh_Contain.Append(" || ");
                }

                List<object> lp = new List<object>();
                predicate = wh_Contain.ToString();
                lp.Add(kw);

                SearchRequest sr = new SearchRequest(selectTop, 1, predicate, lp.Count == 0 ? null : lp.ToArray());
                SearchResult rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    int countPage = (int)(rs.Total / selectTop) + (rs.Total % selectTop == 0 ? 0 : 1);
                    lbl_PageTotal.Text = countPage.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        private void tabPage_Prev(
            string modelName, InfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            SearchRequest sr = page.SearchRequest;
            SearchResult rs = page.SearchResult;
            if (sr == null)
            {
                int PageNumber = page.PageCurrent - 1;
                if (PageNumber == 0) return;

                page.PageCurrent = PageNumber;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                grid.SetDataBinding(info.Fields, db.GetSelectPage(modelName, PageNumber, selectTop));
            }
            else
            {
                sr.PageNumber = rs.PageNumber - 1;
                if (sr.PageNumber == 0) return;

                rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        private void tabPage_Next(
            string modelName, InfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            SearchRequest sr = page.SearchRequest;
            SearchResult rs = page.SearchResult;
            if (sr == null)
            {
                int PageNumber = page.PageCurrent + 1;
                if (PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                page.PageCurrent = PageNumber;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                grid.SetDataBinding(info.Fields, db.GetSelectPage(modelName, PageNumber, selectTop));
            }
            else
            {
                sr.PageNumber = sr.PageNumber + 1;
                if (sr.PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        #endregion


    }
}
