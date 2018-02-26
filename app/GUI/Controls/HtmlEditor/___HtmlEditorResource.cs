using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace app.GUI.Html
{
    public class ___HtmlEditorResource
    {
        const string PATH = "";
        public static Bitmap GetImageIcon(string img_gif)
        {
            string resourceName = @"GUI\Controls\HtmlEditor\Icons\" + img_gif;
            var assembly = Assembly.GetExecutingAssembly();
            resourceName = typeof(App).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                return new Bitmap(stream);
        }
    }
}
