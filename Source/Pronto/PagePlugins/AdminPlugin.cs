using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Pronto.PagePlugins
{
    public class AdminPlugin : PagePluginBase
    {
        public AdminPlugin(WebsiteConfiguration websiteConfiguration)
        {
            this.websiteConfiguration = websiteConfiguration;
        }

        WebsiteConfiguration websiteConfiguration;

        public override IEnumerable<XObject> GetHeadContents(bool firstCall)
        {
            if (firstCall && IsUserAdmin)
            {
                yield return new XElement("link",
                    new XAttribute("href", "~/admin/admin.css"),
                    new XAttribute("type", "text/css"),
                    new XAttribute("rel", "stylesheet")
                );
            }
        }

        public override IEnumerable<XObject> Render(string data)
        {
            if (IsUserAdmin)
            {
                return RenderAdmin();
            }
            else
            {
                return RenderAnonymous();
            }
        }

        IEnumerable<XObject> RenderAdmin()
        {
            yield return InlineScript(AdminWindowCmsJson());
            yield return Script("~/admin/jquery.form-2.28.js");
            yield return Script("~/admin/jquery.jtree.1.0.js");
            yield return Script("~/admin/fckeditor/fckeditor.js");
            yield return Script("~/admin/admin.js");
        }

        IEnumerable<XObject> RenderAnonymous()
        {
            if (ViewContext.HttpContext.User.Identity.IsAuthenticated)
            {
                yield return new XElement("a",
                    new XAttribute("href", Url.RouteUrl("Auth", new { action = "LogOut" })),
                    "Log Out"
                );
            }
            else
            {
                yield return new XElement("a",
                    new XAttribute("id", "cms-admin-link"),
                    new XAttribute("href", "#"),
                    "Log In"
                );
            }

            yield return InlineScript(AnonymousWindowCmsJson());
            yield return Script("~/admin/" + CmsApplication.AuthType + "/anonymous.js");
        }

        string AnonymousWindowCmsJson()
        {
            if (ViewContext.HttpContext.User.Identity.IsAuthenticated)
            {
                return "window.cms = { urlBase: " + UrlBase.ToJavascriptString() + ", isNewUser: true, adminId: " + ViewContext.HttpContext.User.Identity.Name.ToJavascriptString() + " }";
            }
            else
            {
                return "window.cms = { urlBase: " + UrlBase.ToJavascriptString() + " }";
            }
        }

        string AdminWindowCmsJson()
        {
            return
                "window.cms = { page: { path: " + Page.Path.ToJavascriptString() +
                    ", title: " + Page.Title.ToJavascriptString() +
                    ", name: " + Page.Name.ToJavascriptString() +
                    ", description: " + Page.Description.ToJavascriptString() +
                    ", template: " + Page.Template.ToLowerInvariant().ToJavascriptString() +
                    ", navigation: " + (Page.Navigation ? "true" : "false") +
                    " }, urlBase: " + UrlBase.ToJavascriptString() +
                      ", adminId: " + ViewContext.HttpContext.User.Identity.Name.ToJavascriptString() +
                      ", templates: " + GetTemplatesJson() +
                      ", authType: " + CmsApplication.AuthType.ToJavascriptString() + " }";
        }

        string GetTemplatesJson()
        {
            var templates = Directory.GetFiles(websiteConfiguration.TemplateDirectory, "*.htm")
                .Select(f => Path.GetFileName(f))
                .Select(s => s.ToJavascriptString())
                .ToArray();
            return "[" + string.Join(",", templates) + "]";
        }

        static XElement InlineScript(string javascript)
        {
            return new XElement("script",
                new XAttribute("type", "text/javascript"),
                "/*",
                new XCData("*/" + javascript + "//")
            );
        }

        static XElement Script(string src)
        {
            return new XElement("script",
                new XAttribute("type", "text/javascript"),
                new XAttribute("src", src)
            );
        }
    }
}