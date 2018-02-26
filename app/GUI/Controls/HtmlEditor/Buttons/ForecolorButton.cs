using System.Drawing;
using System.Windows.Forms;

namespace app.GUI.Html
{
    public class ForecolorButton : IHTMLEditorButton
    {
        public void IconClicked(HTMLEditorButtonArgs args)
        {
            var colorPicker = new ColorDialog();
            var result = colorPicker.ShowDialog();
            if(result == DialogResult.OK)
            {
                var color = colorPicker.Color;
                var hexcolor = ColorTranslator.ToHtml(color);
                args.Document.ExecCommand("ForeColor", false, hexcolor);
            }
        }

        public Image IconImage
        {
            get { return ___HtmlEditorResource.GetImageIcon("fontforecolorpicker.gif"); }
        }

        public string IconName
        {
            get { return "Color"; }
        }

        public string IconTooltip
        {
            get { return "Color"; }
        }

        public string CommandIdentifier
        {
            get { return "ForeColor"; }
        }
    }
}