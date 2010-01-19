using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Web;

namespace Pronto.Views
{
    public class PageView : IView
    {
        public PageView(XDocument html, Func<string, IPagePlugin> getPlugin)
        {
            this.html = html;
            this.getPlugin = getPlugin;
        }

        XDocument html;
        Func<string, IPagePlugin> getPlugin;

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            List<XElement> scripts = new List<XElement>();
            var html = new XDocument(this.html);
            var page = (IReadOnlyPage)viewContext.ViewData["page"];
            ExpandPlugIns(viewContext, html, scripts);
            AddResetCss(html);
            AddJQuery(html);
            AddPluginScripts(html, scripts);
            if (viewContext.HttpContext.IsDebuggingEnabled == false)
            {
                UseMinifiedJavascript(html, viewContext.HttpContext.Server);
            }
            VersionScriptAndStylesheetUrls(html, viewContext.HttpContext.Server);
            AddPageNameClassToBody(page, html);
            AddDescriptionMetaTag(page, html);
            ExpandEmptyNonSelfClosingElements(html);
            ExpandRelativePaths(html, viewContext.HttpContext.Request);
            RenderHtml(html, viewContext, writer);
        }

        void AddResetCss(XDocument html)
        {
            var link = new XElement("link",
                new XAttribute("href", ResetCssUrl()),
                new XAttribute("type", "text/css"),
                new XAttribute("rel", "stylesheet")
            );
            var head = html.Root.Element("head");
            var title = head.Elements("title").FirstOrDefault();
            if (title == null)
            {
                head.AddFirst(link);
            }
            else
            {
                title.AddAfterSelf(link);
            }
        }

        void ExpandPlugIns(ViewContext viewContext, XDocument html, List<XElement> scripts)
        {
            var pis = html.DescendantNodes().OfType<XProcessingInstruction>().ToArray();
            ExpandPlugins(viewContext, pis, html.Root.Element("head"), scripts);
        }

        void ExpandPlugins(ViewContext viewContext, XProcessingInstruction[] pis, XElement head, List<XElement> scripts)
        {
            var typesUsed = new HashSet<Type>();
            foreach (var pi in pis)
            {
                var plugin = getPlugin(pi.Target);
                plugin.Initialize(viewContext);

                var isFirstUse = !typesUsed.Contains(plugin.GetType());
                var headContents = plugin.GetHeadContents(isFirstUse);
                typesUsed.Add(plugin.GetType());
                head.Add(headContents);

                var content = plugin.Render(pi.Data).ToArray();
                ExpandPlugins(viewContext, content.OfType<XProcessingInstruction>().ToArray(), head, scripts);
                ExpandPlugins(viewContext, content.OfType<XContainer>().DescendantNodes().OfType<XProcessingInstruction>().ToArray(), head, scripts);
                pi.ReplaceWith(content);

                scripts.AddRange(plugin.GetScripts(isFirstUse));
            }
        }
        
        void ExpandEmptyNonSelfClosingElements(XDocument html)
        {
            var selfClosingElements = new HashSet<string> { "area", "base", "basefont", "br", "hr", "img", "input", "link", "meta" };
            var elementsToExpand = html.Descendants().Where(element => 
                element.IsEmpty && !selfClosingElements.Contains(element.Name.LocalName)
            );
            foreach (var element in elementsToExpand)
            {
                element.Add(new XText(""));
            }
        }

        void ExpandRelativePaths(XDocument html, HttpRequestBase request)
        {
            var root = request.ApplicationPath.TrimEnd('/');
            foreach (var element in html.Descendants())
            {
                XAttribute attribute = null;
                var name = element.Name.LocalName;

                if (name == "script" || name == "img")
                {
                    attribute = element.Attribute("src");
                }
                else if (name == "a" || name == "link")
                {
                    attribute = element.Attribute("href");
                }
                else if (name == "form")
                {
                    attribute = element.Attribute("action");
                }

                if (attribute != null && attribute.Value.StartsWith("~"))
                {
                    attribute.Value = root + attribute.Value.Substring(1);
                }
            }
        }

        void AddJQuery(XDocument html)
        {
            var jquery = new XElement("script", 
                new XAttribute("type", "text/javascript"),
                new XAttribute("src", JQueryJavascriptUrl()));

            var jqueryUI = new XElement("script", 
                new XAttribute("type", "text/javascript"),
                new XAttribute("src", JQueryUIJavascriptUrl()));

            var firstScript = html.Descendants("script").FirstOrDefault();
            if (firstScript == null)
            {
                html.Root.Element("body").Add(jquery);
                html.Root.Element("body").Add(jqueryUI);
            }
            else
            {
                firstScript.AddBeforeSelf(jquery);
                firstScript.AddBeforeSelf(jqueryUI);
            }

            var head = html.Root.Element("head");
            head.Elements("link").First().AddAfterSelf(
                new XElement("link",
                    new XAttribute("href", JQueryUICssUrl()),
                    new XAttribute("type", "text/css"), 
                    new XAttribute("rel", "stylesheet")
                )
            );
        }

        void AddPluginScripts(XDocument html, List<XElement> scripts)
        {
            var lastScript = html.Root.Descendants("script").Last();
            foreach (var script in Enumerable.Reverse(scripts))
            {
                lastScript.AddAfterSelf(script);
            }
        }
        
        void UseMinifiedJavascript(XDocument html, HttpServerUtilityBase server)
        {
            foreach (var script in html.Descendants("script")
                .Where(s => s.Attribute("src") != null
                    && !s.Attribute("src").Value.StartsWith("http:") 
                    && !s.Attribute("src").Value.StartsWith("https:")))
            {
                var src = script.Attribute("src").Value;
                var minSrc = Path.ChangeExtension(src, ".min.js");
                if (File.Exists(server.MapPath(minSrc)))
                {
                    script.Attribute("src").Value = minSrc;
                }
            }
        }

        void VersionScriptAndStylesheetUrls(XDocument html, HttpServerUtilityBase server)
        {
            var srcs = from element in html.Descendants("script")
                let attr = element.Attribute("src")
                where attr != null
                select attr;

            var hrefs = from element in html.Descendants("link")
                let typeAttr = element.Attribute("type")
                let hrefAttr = element.Attribute("href")
                where typeAttr != null && hrefAttr != null && typeAttr.Value == "text/css"
                select hrefAttr;

            foreach (var attr in srcs.Concat(hrefs).Where(a => 
                !(a.Value.StartsWith("http:") || a.Value.StartsWith("https:"))))
            {
                var url = attr.Value;
                var filename = server.MapPath(url);
                if (File.Exists(filename))
                {
                    if (url.Contains("?"))
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += "_version=" + File.GetLastWriteTimeUtc(filename).Ticks;
                    attr.Value = url;
                }
            }
        }

        static string ResetCssUrl()
        {
            var url = WebConfigurationManager.AppSettings["reset-css"];
            return url ?? "http://yui.yahooapis.com/2.7.0/build/reset/reset-min.css";
        }

        static string JQueryJavascriptUrl()
        {
            var url = WebConfigurationManager.AppSettings["jquery-js"];
            return url ?? "http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
        }

        static string JQueryUIJavascriptUrl()
        {
            var url = WebConfigurationManager.AppSettings["jquery-ui-js"];
            return url ?? "http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/jquery-ui.min.js";
        }

        static string JQueryUICssUrl()
        {
            var url = WebConfigurationManager.AppSettings["jquery-ui-css"];
            return url ?? "http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/ui-lightness/jquery-ui.css";
        }

        void AddPageNameClassToBody(IReadOnlyPage page, XDocument html)
        {
            var path = page.Path.Replace('/','_');
            var cssClass = "page-" + path;

            var body = html.Root.Element("body");
            if (body.Attribute("class") == null)
            {
                body.Add(new XAttribute("class", cssClass));
            }
            else
            {
                body.Attribute("class").Value += " " + cssClass;
            }
        }

        void AddDescriptionMetaTag(IReadOnlyPage page, XDocument html)
        {
            if (string.IsNullOrEmpty(page.Description)) return;

            var head = html.Root.Element("head");
            var title = head.Element("title");
            var meta = new XElement("meta", 
                new XAttribute("name", "description"), 
                new XAttribute("content", page.Description));
            if (title != null)
            {
                title.AddAfterSelf(meta);
            }
            else
            {
                head.AddFirst(meta);
            }
        }

        void RenderHtml(XDocument html, ViewContext viewContext, TextWriter writer)
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = viewContext.HttpContext.IsDebuggingEnabled,
            };

            // Write DOCTYPE manually because XmlEncodedRawTextWriter writes "<!DOCTYPE html >"
            // We don't want the space before the ">".
            writer.WriteLine("<!DOCTYPE html>");
            using (var xmlWriter = XmlWriter.Create(writer, settings))
            {
                html.WriteTo(xmlWriter);
            }
        }
    }
}
