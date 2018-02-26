using System.Drawing;
using Microsoft.VisualBasic; 
using System.Windows.Forms;
using System.IO;
using System;

namespace app.GUI.Html
{
    public class WordButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            var x = args.Editor.Location.X + 10;
            var y = args.Editor.Location.Y + 10;

            //var url = Interaction.InputBox("Please enter an image url", "URL", null, x, y);
            //if (!string.IsNullOrEmpty(url))
            //{
            //    args.Document.ExecCommand("InsertImage", false, url);
            //}

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;//, file_name = ""; 
                args.Document.ExecCommand("InsertImage", false, file);
            }

            ////OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //////openFileDialog1.InitialDirectory = "c:\\";
            ////openFileDialog1.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
            ////openFileDialog1.FilterIndex = 2;
            ////openFileDialog1.RestoreDirectory = true;

            ////if (openFileDialog1.ShowDialog() == DialogResult.OK)
            ////{
            ////    string file = openFileDialog1.FileName, file_name = "";
            ////    string[] a = file.Split('\\');
            ////    file_name = "/images/[host]/" + a[a.Length - 1];

            ////    string s64 = "";
            ////    using (Image image = Image.FromFile(file))
            ////    {
            ////        using (MemoryStream ms = new MemoryStream())
            ////        {
            ////            image.Save(ms, image.RawFormat);
            ////            byte[] imageBytes = ms.ToArray();
            ////            s64 = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
            ////            //s64 = Convert.ToBase64String(imageBytes);
            ////        }
            ////    }

            ////    args.Document.ExecCommand("InsertImage", false, s64);
            ////}


        } //end function

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("word.png"); }
        }

        public string IconName
        {
            get { return "Upload content from file word"; }
        }

        public string IconTooltip
        {
            get { return "Upload content from file word"; }
        }

        public string CommandIdentifier
        {
            get { return "InsertImage"; }
        }
    }
}