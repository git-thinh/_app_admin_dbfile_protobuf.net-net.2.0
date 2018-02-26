using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sano.PersonalProjects.ColorPicker.Controls;
using System.Drawing;
using app.Core;
using Deveck.Ui.Controls;
using Deveck.Ui.Controls.Scrollbar;
using System.Linq.Dynamic;
using System.Collections;

namespace app.GUI
{
    public class FormLookupItem : FormBase
    {
        public FormLookupItem(IDataFile db)
            : base("LOOKUP ITEM", true)
        {
            FormBorderStyle = FormBorderStyle.None;
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(800, 400);

            string dbNameCurrent = "Test";
            int selectTop = 100;
            string[] OpString = "Equals,NotEquals,Contains,StartsWith,EndsWith".Split(',');
            string[] OpNumber = "Equals,NotEquals,Contains,GreaterThan,LessThan,GreaterThanOrEqual,LessThanOrEqual".Split(',');

            var info = db.GetInfoSelectTop(dbNameCurrent, selectTop);
            if (info != null)
            {
                #region [ === CREATE NEW TAB PAGE === ]

                PanelCustom page = new PanelCustom(info.Fields) { Dock = DockStyle.Fill, BackColor = Color.White, };

                int hi = (((info.Fields.Length / 3) + 1) * 25);
                if (info.Fields.Length < 4) hi = 50;
                FlowLayoutPanel boi_Filter = new FlowLayoutPanel() { Dock = DockStyle.Top, Height = hi, AutoScroll = false, Padding = new Padding(0), BackColor = App.ColorBg };
                boi_Filter.FlowDirection = FlowDirection.LeftToRight;
                boi_Filter.MouseDown += FormMove_MouseDown;

                Label lbl_Keyword = new Label() { Width = 60, AutoSize = false, Text = "Search ", ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight };
                TextBox txt_Keyword = new TextBox() { Width = 166, BorderStyle = BorderStyle.FixedSingle };
                Label sp0 = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                boi_Filter.Controls.AddRange(new Control[]{
                        lbl_Keyword,
                        txt_Keyword,
                        sp0
                    });

                StringBuilder wh_Contain = new StringBuilder();
                for (int k = 0; k < info.Fields.Length; k++)
                {
                    var dp = info.Fields[k];
                    wh_Contain.Append(dp.NAME + (dp.Type.Name == "String" ? string.Empty : ".ToString()") + ".Contains(@0) ");
                    if (k < info.Fields.Length - 1) wh_Contain.Append(" || ");

                    Label lbl = new Label() { Name = "lbl" + k.ToString(), Text = dp.NAME, AutoSize = false, Width = 60, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight };
                    ComboBox cbo = new ComboBox() { Name = "cbo" + k.ToString(), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList, };
                    if (dp.Type.Name == "String")
                        for (int ki = 0; ki < OpString.Length; ki++) cbo.Items.Add(OpString[ki]);
                    else
                        for (int ki = 0; ki < OpNumber.Length; ki++) cbo.Items.Add(OpNumber[ki]);
                    TextBox txt = new TextBox() { Name = "txt" + k.ToString(), Width = 80, BorderStyle = BorderStyle.FixedSingle };
                    boi_Filter.Controls.AddRange(new Control[] {
                            lbl,
                            cbo,
                            txt,
                        });

                    if (k != 0 && k % 3 == 0)
                    {
                        Label sp = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                        boi_Filter.Controls.Add(sp);
                    }
                }//end for fields

                Panel boi_Action = new Panel() { Dock = DockStyle.Top, Height = 27, AutoScroll = false, Padding = new Padding(0, 5, 10, 3), BackColor = App.ColorBg };
                boi_Action.MouseDown += FormMove_MouseDown;

                Label lbl_TotalRecord = new Label() { Dock = DockStyle.Left, Text = "Records: " + info.DataSelectTop.Count.ToString() + " / " + info.TotalRecord.ToString() + " ", AutoSize = false, Width = 199, Padding = new Padding(0), Font = new Font(new FontFamily("Arial"), 8F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.BottomLeft };
                Label lbl_Port = new Label() { Dock = DockStyle.Left, Text = "Port HTTP: " + info.PortHTTP.ToString(), AutoSize = false, Width = 110, Padding = new Padding(4, 0, 0, 0), Font = new Font(new FontFamily("Arial"), 8F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.BottomLeft };
                ButtonCustom btn_Search = new ButtonCustom() { Text = "search", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                ButtonCustom btn_PagePrev = new ButtonCustom() { Text = "<<<", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                ButtonCustom btn_PageNext = new ButtonCustom() { Text = ">>>", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                Label spa = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                Label lbl_PageCurrent = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                Label lbl_PageSP = new Label() { Text = " | ", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                Label lbl_PageTotal = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                lbl_PageTotal.Text = ((int)(info.TotalRecord / selectTop) + (info.TotalRecord % selectTop == 0 ? 0 : 1)).ToString();

                boi_Action.Controls.AddRange(new Control[] {
                        lbl_TotalRecord,
                        lbl_Port,
                        btn_PagePrev,
                        lbl_PageCurrent,
                        lbl_PageSP,
                        lbl_PageTotal,
                        btn_PageNext,
                        btn_Search,
                        spa,
                    });

                Panel boi_Data = new Panel() { Dock = DockStyle.Fill };

                CustomScrollbar scrollGrig = new CustomScrollbar()
                {
                    Dock = DockStyle.Right,

                    ActiveBackColor = Color.White,
                    BackColor = Color.White,

                    LargeChange = 10,
                    Location = new Point(306, 12),
                    Maximum = 99,
                    Minimum = 0,
                    Size = new Size(13, 303),
                    SmallChange = 1,
                    TabIndex = 1,
                    ThumbStyle = CustomScrollbar.ThumbStyleEnum.Auto,
                    Value = 0,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                //ScrollbarStyleHelper.ApplyStyle(scrollGrig, ScrollbarStyleHelper.StyleTypeEnum.Blue);
                //ScrollbarStyleHelper.ApplyStyle(scrollGrig, ScrollbarStyleHelper.StyleTypeEnum.Black);

                CustomListView grid = new CustomListView()
                {
                    VScrollbar = new ScrollbarCollector(scrollGrig),

                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    // Set the view to show details.
                    View = View.Details,
                    // Allow the user to edit item text.
                    //LabelEdit = true,
                    // Allow the user to rearrange columns.
                    //AllowColumnReorder = true,
                    // Select the item and subitems when selection is made.
                    FullRowSelect = true,
                    // Display grid lines.
                    GridLines = true,
                    // Sort the items in the list in ascending order.
                    //Sorting = SortOrder.Ascending,


                    //HeaderStyle = ColumnHeaderStyle.None,
                    //FullRowSelect = true,
                    //HideSelection = false,
                    Location = new Point(12, 12),
                    MultiSelect = false,
                    Size = new Size(288, 303),
                    UseCompatibleStateImageBehavior = false,
                    //View = View.Details,
                    HeaderStyle = ColumnHeaderStyle.Nonclickable,
                }; ;
                grid.SetDataBinding(info.Fields, info.DataSelectTop);

                boi_Data.Controls.AddRange(new Control[] {
                        grid,
                        scrollGrig,
                    });

                page.Controls.AddRange(new Control[] {
                        boi_Filter,
                        boi_Action,
                        boi_Data,
                    });

                this.Controls.Add(page);
                boi_Filter.BringToFront();
                boi_Action.BringToFront();
                //grid.BringToFront();
                boi_Data.BringToFront();

                PanelHeader_SendToBack();

                #endregion

                //////////////////////////////////////////////////

                #region [ === PREV - NEXT === ]

                btn_PageNext.Click += (se, ev) =>
                {
                    SearchRequest sr = page.SearchRequest;
                    SearchResult rs = page.SearchResult;
                    if (sr == null)
                    {
                        int PageNumber = page.PageCurrent + 1;
                        if (PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                        page.PageCurrent = PageNumber;
                        lbl_PageCurrent.Text = page.PageCurrent.ToString();
                        grid.SetDataBinding(info.Fields, db.GetSelectPage(dbNameCurrent, PageNumber, selectTop));
                    }
                    else
                    {
                        sr.PageNumber = sr.PageNumber + 1;
                        if (sr.PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                        rs = db.Search(dbNameCurrent, sr);

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
                };

                btn_PagePrev.Click += (se, ev) =>
                {
                    SearchRequest sr = page.SearchRequest;
                    SearchResult rs = page.SearchResult;
                    if (sr == null)
                    {
                        int PageNumber = page.PageCurrent - 1;
                        if (PageNumber == 0) return;

                        page.PageCurrent = PageNumber;
                        lbl_PageCurrent.Text = page.PageCurrent.ToString();
                        grid.SetDataBinding(info.Fields, db.GetSelectPage(dbNameCurrent, PageNumber, selectTop));
                    }
                    else
                    {
                        sr.PageNumber = rs.PageNumber - 1;
                        if (sr.PageNumber == 0) return;

                        rs = db.Search(dbNameCurrent, sr);

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
                };

                #endregion

                //////////////////////////////////////////////////

                #region [ === SEARCH === ]

                txt_Keyword.KeyDown += (se, ev) =>
                {
                    if (ev.KeyCode == Keys.Enter)
                    {
                        //grid.DataSource = null;

                        string predicate = "", kw = txt_Keyword.Text;
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
                            List<object> lp = new List<object>();
                            predicate = wh_Contain.ToString();
                            lp.Add(kw);

                            SearchRequest sr = new SearchRequest(selectTop, 1, predicate, lp.Count == 0 ? null : lp.ToArray());
                            SearchResult rs = db.Search(dbNameCurrent, sr);

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
                };

                btn_Search.Click += (se, ev) =>
                {
                    //grid.DataSource = null;

                    string predicate = "", kw = txt_Keyword.Text;
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
                        List<object> lp = new List<object>();
                        predicate = wh_Contain.ToString();
                        lp.Add(kw);

                        SearchRequest sr = new SearchRequest(selectTop, 1, predicate, lp.Count == 0 ? null : lp.ToArray());
                        SearchResult rs = db.Search(dbNameCurrent, sr);

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
                };

                #endregion

            }//end bind info Model
             
        }


    }
}
