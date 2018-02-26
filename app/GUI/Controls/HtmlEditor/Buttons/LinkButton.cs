using System.Drawing;

namespace app.GUI.Html
{
    public class LinkButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("CreateLink", true, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("createlink.gif"); }
        }

        public string IconName
        {
            get { return "Create link"; }
        }

        public string IconTooltip
        {
            get { return "Create link"; }
        }

        public string CommandIdentifier
        {
            get { return "CreateLink"; }
        }
    }
}