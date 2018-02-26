using System;
using System.Drawing;

namespace app.GUI.Html
{
    public class UnderlineButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("Underline", false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("underline.gif"); }
        }

        public string IconName
        {
            get { return "Underline"; }
        }

        public string IconTooltip
        {
            get { return "Underline"; }
        }

        public string CommandIdentifier
        {
            get { return "Underline"; }
        }
    }
}