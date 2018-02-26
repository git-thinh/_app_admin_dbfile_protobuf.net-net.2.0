using System.Drawing;

namespace app.GUI.Html
{
    public class JustifyRightButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand(CommandIdentifier, false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("justifyright.gif"); }
        }

        public string IconName
        {
            get { return "Justify right"; }
        }

        public string IconTooltip
        {
            get { return "Justify right"; }
        }

        public string CommandIdentifier
        {
            get { return "JustifyRight"; }
        }
    }
}