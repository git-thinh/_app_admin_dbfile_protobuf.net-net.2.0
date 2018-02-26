using System;
using System.Drawing;

namespace app.GUI.Html
{
    public class OrderedListButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("InsertOrderedList", false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("numberedlist.gif"); }
        }

        public string IconName
        {
            get { return "Ordered list"; }
        }

        public string IconTooltip
        {
            get { return "Ordered list"; }
        }

        public string CommandIdentifier
        {
            get { return "InsertOrderedList"; }
        }
    }
}