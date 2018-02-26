using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Deveck.Ui.Controls.Scrollbar;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms.VisualStyles;
using app.Core;

namespace Deveck.Ui.Controls
{
    public class ListViewModelItem : ListView
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;
        private const int SB_CTL = 2;
        private const int SB_BOTH = 3;

        public delegate void EventItemClick(int index, object value);
        public EventItemClick OnItemClick;

        public ListViewModelItem()
        {
            CheckBoxes = true;
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            BorderStyle = BorderStyle.None;

            View = View.Details;
            FullRowSelect = true;
            GridLines = true;
            Location = new Point(12, 12);
            MultiSelect = false;
            Size = new Size(288, 303);
            UseCompatibleStateImageBehavior = false;

            OwnerDraw = true;
            ColumnClick += (se, e) =>
            {
                #region
                if (e.Column == 0)
                {
                    bool value = false;
                    try
                    {
                        value = Convert.ToBoolean(this.Columns[e.Column].Tag);
                    }
                    catch (Exception)
                    {
                    }
                    this.Columns[e.Column].Tag = !value;
                    foreach (ListViewItem item in this.Items)
                        item.Checked = !value;

                    this.Invalidate();
                }
                #endregion
            };
            DrawColumnHeader += (se, e) =>
            {
                #region
                e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);

                if (e.ColumnIndex == 0)
                {
                    bool value = false;
                    try
                    {
                        value = Convert.ToBoolean(e.Header.Tag);
                    }
                    catch (Exception)
                    {
                    }
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 4, e.Bounds.Top + 4),
                        value ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);
                    return;
                }

                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                using (Font headerFont = new Font("Arial", 8, FontStyle.Regular))
                    e.Graphics.DrawString(e.Header.Text, headerFont, Brushes.Black, new Rectangle(e.Bounds.X, e.Bounds.Y + 3, e.Bounds.Width, e.Bounds.Height - 3), sf);
                #endregion
            };
            DrawSubItem += (se, e) =>
            {
                #region
                bool selected = (e.ItemState & ListViewItemStates.Selected) != 0;

                if (selected)
                    e.Graphics.FillRectangle(Brushes.Orange, e.Bounds);
                else if (e.ItemIndex % 2 != 0)
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), e.Bounds);

                switch (e.ColumnIndex)
                {
                    case 0:
                        e.DrawDefault = true;
                        break;
                    case 1:
                        using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                        using (Font headerFont = new Font("Arial", 7, FontStyle.Regular))
                            e.Graphics.DrawString(e.SubItem.Text, headerFont, Brushes.Gray, new Point(e.Bounds.X + 10, e.Bounds.Y + 3), sf);
                        break;
                    default:
                        //e.DrawDefault = true; 
                        e.DrawText();

                        //bool value = false;
                        //try
                        //{
                        //    value = Convert.ToBoolean(e.Header.Tag);
                        //}
                        //catch (Exception)
                        //{
                        //}
                        //CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 4, e.Bounds.Top + 4),
                        //    value ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

                        //////using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                        //////using (Font headerFont = new Font("Arial", 8, FontStyle.Regular)) //Font size!!!! 
                        //////    e.Graphics.DrawString(e.SubItem.Text, headerFont, Brushes.Black, 
                        //////        new Rectangle(e.Bounds.X, e.Bounds.Y + 3, e.Bounds.Width, e.Bounds.Height + 3), sf);


                        break;
                }


                #endregion
            };
            DrawItem += (se, e) =>
            {
            };
            MouseClick += (se, ev) =>
            {
                int index = -1;
                for (int i = 0; i < this.Items.Count; i++) 
                    if (this.GetItemRect(i).Contains(ev.Location))
                    {
                        index = i;
                        break;
                    }
                if (index != -1) 
                {
                    ViewModelItem item = this.Items[index] as ViewModelItem;
                    if (OnItemClick != null) 
                        OnItemClick(index, item.Value);
                }
            };
        }

        //global brushes with ordinary/selected colors
        private readonly SolidBrush reportsForegroundBrushSelected = new SolidBrush(Color.White);
        private readonly SolidBrush reportsForegroundBrush = new SolidBrush(Color.Black);
        private readonly SolidBrush reportsBackgroundBrushSelected = new SolidBrush(Color.FromKnownColor(KnownColor.Highlight));
        private readonly SolidBrush reportsBackgroundBrush1 = new SolidBrush(Color.White);
        private readonly SolidBrush reportsBackgroundBrush2 = new SolidBrush(Color.Gray);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m_CountItem < 10)
                ShowScrollBar(this.Handle, SB_VERT, false);
            else
                ShowScrollBar(this.Handle, SB_VERT, true);
        }

        const int m_Width_Col_INT = 120;
        const int m_Width_Col_STRING = 250;
        private int m_CountItem = 0;
        private int _paddingForm = 4;
        private int _scrollWidth = 0;
        public void SetDataBinding(FieldInfo[] fields, IList dataSource)
        {
            m_CountItem = dataSource.Count;
            this.Clear();

            int _colCheckBox = 20;
            int _colIndex = 26;
            this.Columns.Add(new ColumnHeader
            {
                Text = string.Empty,
                Width = _colCheckBox,
                TextAlign = HorizontalAlignment.Center,
            });
            this.Columns.Add(new ColumnHeader
            {
                Text = string.Empty,
                Width = _colIndex,
                TextAlign = HorizontalAlignment.Center,
            });

            int wBlank = _colCheckBox + _colIndex;
            foreach (var col in fields)
            {
                int wi = m_Width_Col_INT;
                //if (col.Name.ToLower() == "id") wi = 55;
                HorizontalAlignment textAlign = HorizontalAlignment.Center;
                if (col.Type.Name == "String")
                {
                    wi = m_Width_Col_STRING;
                    textAlign = HorizontalAlignment.Left;
                }
                this.Columns.Add(new ColumnHeader
                {
                    Text = col.NAME.ToUpper(),
                    Width = wi,
                    TextAlign = textAlign,
                    Tag = col,
                });
                wBlank += wi;
            }
            if (m_CountItem < 10)
                _scrollWidth = 0;
            else
                _scrollWidth = SystemInformation.VerticalScrollBarWidth;

            //wBlank = (app.App.Width - (app.App.col_Left_Width + app.App.col_Splitter_Width + _paddingForm + _scrollWidth)) - wBlank;
            int wiCheck = (app.App.Width - (app.App.col_Left_Width + app.App.col_Splitter_Width + _paddingForm + _scrollWidth)) - wBlank;
            if (wiCheck < 44) wiCheck = 44;
            this.Columns.Add(new ColumnHeader { Text = string.Empty, Width = wiCheck, TextAlign = HorizontalAlignment.Center });

            string[] cols = fields.Select(x => x.NAME).ToArray();

            int k = 0;
            int sumCOL = fields.Length + 3;
            foreach (object o in dataSource)
            {
                var items = new string[sumCOL];
                items[0] = string.Empty;
                items[1] = (k + 1).ToString();
                 
                for (int i = 2; i < sumCOL - 1; i++)
                {
                    var po = o.GetType().GetProperty(cols[i - 2]);
                    object val = po.GetValue(o, null);
                    items[i] = val == null ? string.Empty : Convert.ToString(val); 
                }

                items[sumCOL - 1] = string.Empty; // Value column check

                this.Items.Add(new ViewModelItem(items) { Value = o });
                if (k % 2 != 0) this.Items[k].BackColor = SystemColors.Control;
                k++;
            }
        }
    }

    public class ViewModelItem : ListViewItem
    {
        public object Value { set; get; } 

        public ViewModelItem(string[] items)
            : base(items)
        {

        }
    }
}
