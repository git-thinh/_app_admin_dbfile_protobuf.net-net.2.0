using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace app.GUI
{ 
    public partial class ucDataSplitConfig : UserControl
    {
        private TextBoxCustom txt_Text;
        private TextBoxCustom txt_SkipLine;

        public ucDataSplitConfig(string waterMarkText, string checkText)
        {
            this.Font = App.Font;
            txt_SkipLine = new TextBoxCustom() { Dock = DockStyle.Left, Width = 60, Text = "0", WaterMark = "Skip line", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            txt_SkipLine.KeyPress += (se, ev) => 
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                    ev.Handled = true;
            };
            txt_Text = new TextBoxCustom() { Dock = DockStyle.Fill, WaterMark = waterMarkText, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            this.Controls.AddRange(new Control[] { txt_SkipLine, txt_Text });
            txt_Text.BringToFront();
        }

        public void Reset()
        {
            txt_Text.Text = "";
            txt_SkipLine.Text = "0";
        }

        public ConfigItem Value
        {
            get
            {
                if (int.Parse(txt_SkipLine.Text) > 255) return new ConfigItem() { SkipLine = 0, Text = txt_Text.Text };
                return new ConfigItem() { SkipLine = txt_SkipLine.Text == "" ? (byte)0 : byte.Parse(txt_SkipLine.Text), Text = txt_Text.Text };
            }
            set
            {
                txt_SkipLine.Text = value.SkipLine.ToString();
                txt_Text.Text = value.Text;
            }
        }
    }

    public class ConfigItem
    {
        public byte SkipLine { set; get; }
        public string Text { set; get; }

        public ConfigItem() { }
        public ConfigItem(string text) {
            string sk = text.Split('|')[0].Trim();
            Text = text.Substring(sk.Length + 2, text.Length - (sk.Length + 2)).Trim();
            byte skip = 0;
            byte.TryParse(sk, out skip);
            SkipLine = skip;
        }
    }

    public class IndexLine
    {
        public int Index { set; get; }
        public string Line { set; get; }
        public IndexLine(int index, string line) { Index = index; Line = line; }
        public override string ToString()
        {
            return string.Format("{0} - {1}", Index, Line);
        }
    }

    public class CmsExtract
    {
        private string tit = "";
        public string Title
        {
            set
            {
                if (value != null)
                {
                    tit = value;
                    tit = tit.Replace("'", string.Empty).Replace("\"", string.Empty);
                }
            }
            get { return tit; }
        }
        private string des = "";
        public string Description
        {
            set
            {
                if (value != null)
                {
                    des = value;
                    des = des.Replace("'", string.Empty).Replace("\"", string.Empty);
                }
            }
            get { return des; }
        }


        public string ContentHtml { set; get; }
        public string ImgDefault { set; get; }

        public string URL { set; get; }
        public string[] IMG_SRC { set; get; }
        public string PLAIN_TEXT { set; get; }

        public CmsExtract()
        {
            Title = "";
            Description = "";
            ContentHtml = "";
            ImgDefault = "";

            URL = "";
            IMG_SRC = new string[] { };
            PLAIN_TEXT = "";
        }
    }

    class oTagSplit
    {
        public string Domain { set; get; }
        public string TrimLeft { set; get; }
        public string TrimRight { set; get; }
        public int TrimRowLeft { set; get; }
        public int TrimRowRight { set; get; }
    } 
}
