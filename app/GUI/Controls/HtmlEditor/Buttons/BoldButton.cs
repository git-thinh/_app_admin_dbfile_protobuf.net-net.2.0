using System.Drawing;

namespace app.GUI.Html
{
    public class BoldButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            args.Document.ExecCommand("Bold", false, null);
        }

        public Image IconImage
        {
            get
            {
                return ___HtmlEditorResource.GetImageIcon("bold.gif");
            }
        }

        public string IconName
        {
            get { return "Bold"; }
        }

        public string IconTooltip
        {
            get { return "Bold"; }
        }

        public string CommandIdentifier
        {
            get { return "Bold"; }
        }
    }
}