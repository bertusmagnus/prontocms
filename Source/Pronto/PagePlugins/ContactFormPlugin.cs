using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;

namespace Pronto.PagePlugins
{
    public class ContactFormPlugin : PagePluginBase
    {
        public override IEnumerable<XObject> Render(string data)
        {
            var sendSuccess = Page.GetContent("send-success").ParseXObjects();
            var sendUrl = Url.RouteUrl("Plugin", new { controller = "contactform", action = "send" });

            yield return new XElement("form", 
                new XAttribute("id", "contact-form"), 
                new XAttribute("method", "post"), 
                new XAttribute("action", sendUrl),
                new XElement("div", new XAttribute("class", "field"),
                    new XElement("label", new XAttribute("for", "contact-form-name"), "Name"),
                    new XElement("input", new XAttribute("id", "contact-form-name"), new XAttribute("name", "name"), new XAttribute("type", "text"))
                ),
                new XElement("div", new XAttribute("class", "field"),
                    new XElement("label", new XAttribute("for", "contact-form-email-address"), "Email Address"),
                    new XElement("input", new XAttribute("id", "contact-form-email-address"), new XAttribute("name", "emailAddress"), new XAttribute("type", "text"))
                ),
                new XElement("div", new XAttribute("class", "field"),
                    new XElement("label", new XAttribute("for", "contact-form-message"), "Message"),
                    new XElement("textarea", new XAttribute("id", "contact-form-message"), new XAttribute("name", "message"), new XAttribute("type", "text"), new XAttribute("rows", "10"), new XAttribute("cols", "30"))
                ),
                new XElement("div", new XAttribute("class", "buttons"),
                    new XElement("input", new XAttribute("type", "submit"), new XAttribute("value", "Send"))
                )
            );
            yield return new XElement("div", 
                new XAttribute("id", "contact-form-send-success"),
                new XAttribute("style", "display:none"),
                sendSuccess
            );
        }

        public override IEnumerable<XElement> GetScripts(bool firstUse)
        {
            if (firstUse)
            {
                yield return new XElement("script", 
                    new XAttribute("type", "text/javascript"), 
                    new XAttribute("src", "~/_plugins/contactform/clientscript")
                );
            }
            else
            {
                yield break;
            }
        }
    }
}
