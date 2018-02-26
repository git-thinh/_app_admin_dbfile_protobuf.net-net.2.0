using System.Drawing;

namespace app.GUI.Html
{
    public class UnorderedListButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("InsertUnorderedList", false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("bulletedlist.gif"); }
        }

        public string IconName
        {
            get { return "Unordered list"; }
        }

        public string IconTooltip
        {
            get { return "Unordered list"; }
        }

        public string CommandIdentifier
        {
            get { return "InsertUnorderedList"; }
        }
    }
}