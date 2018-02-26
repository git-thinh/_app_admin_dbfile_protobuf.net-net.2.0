using System.Drawing;

namespace app.GUI.Html
{
    public class JustifyLeftButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand(CommandIdentifier, false, null);
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("justifyleft.gif"); }
        }

        public string IconName
        {
            get { return "Justify left"; }
        }

        public string IconTooltip
        {
            get { return "Justify left"; }
        }

        public string CommandIdentifier
        {
            get { return "JustifyLeft"; }
        }
    }
}