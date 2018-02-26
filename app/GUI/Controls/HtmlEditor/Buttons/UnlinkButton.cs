using System.Drawing;

namespace app.GUI.Html
{
    public class UnlinkButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("Unlink", false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("unlink.gif"); }
        }

        public string IconName
        {
            get { return "Unlink"; }
        }

        public string IconTooltip
        {
            get { return "Unlink"; }
        }

        public string CommandIdentifier
        {
            get { return "Unlink"; }
        }
    }
}