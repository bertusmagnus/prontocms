using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Xml.Linq;

namespace Pronto.PagePlugins
{
    public class ThemePlugin : PagePluginBase
    {
        public override IEnumerable<XObject> Render(string data)
        {
            var args = data.Split(' ');
            if (args.Length == 0) throw new ArgumentException("Invalid arguments for Theme plugin. Filename must be specified e.g. <?theme myfile.css?>");

            var url = UrlBase + "_theme/" + args[0];
            yield return new XElement("link",
                new XAttribute("href", url),
                new XAttribute("type", "text/css"),
                new XAttribute("rel", "stylesheet"),
                (args.Length > 1 ? new XAttribute("media", args[1]) : null)
            );
        }
    }
}
