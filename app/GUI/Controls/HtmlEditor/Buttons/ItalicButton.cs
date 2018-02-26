using System;
using System.Drawing;
using System.Windows.Forms;

namespace app.GUI.Html
{
    public class ItalicButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("Italic", false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("italic.gif"); }
        }

        public string IconName
        {
            get { return "Italic"; }
        }

        public string IconTooltip
        {
            get { return "Italic"; }
        }

        public string CommandIdentifier
        {
            get { return "Italic"; }
        }
    }
}