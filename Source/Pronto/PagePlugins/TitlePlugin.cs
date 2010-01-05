using System.Collections.Generic;
using System.Xml.Linq;

namespace Pronto.PagePlugins
{
    public class TitlePlugin : PagePluginBase
    {
        public override IEnumerable<XObject> Render(string data)
        {
            if (data == "page")
            {
                yield return new XText(Page.Title);
            }
            else
            {
                var title = Website.Title;
                if (Page.Title.Length > 0 && data != "website")
                {
                    title += " » " + Page.Title;
                }
                yield return new XText(title);
            }
        }
    }
}
