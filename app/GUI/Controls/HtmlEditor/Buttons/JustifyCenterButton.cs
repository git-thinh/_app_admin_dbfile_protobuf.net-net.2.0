using System.Drawing;

namespace app.GUI.Html
{
    public class JustifyCenterButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand(CommandIdentifier, false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("justifycenter.gif"); }
        }

        public string IconName
        {
            get { return "Justify center"; }
        }

        public string IconTooltip
        {
            get { return "Justify center"; }
        }

        public string CommandIdentifier
        {
            get { return "JustifyCenter"; }
        }
    }
}