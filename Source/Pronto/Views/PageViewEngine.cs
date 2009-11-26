using System;
using System.Linq;
using System.IO;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Pronto.Views
{
    public class PageViewEngine : IViewEngine
    {
        public PageViewEngine(WebsiteConfiguration websiteConfiguration, Func<string, IPagePlugin> getPlugin)
        {
            this.websiteConfiguration = websiteConfiguration;
            this.getPlugin = getPlugin;
        }

        WebsiteConfiguration websiteConfiguration;
        Func<string, IPagePlugin> getPlugin;

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            throw new NotImplementedException();
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var pageView = GetPageViewFromCache(controllerContext, viewName);
            if (pageView != null)
            {
                return new ViewEngineResult(pageView, this);
            }
            else
            {
                return FindPageView(controllerContext, viewName);
            }
        }

        ViewEngineResult FindPageView(ControllerContext controllerContext, string viewName)
        {
            var templateFilename = Path.Combine(websiteConfiguration.TemplateDirectory, viewName);
            if (File.Exists(templateFilename))
            {
                var pageView = CreateAndCachePageView(controllerContext, viewName, templateFilename);
                return new ViewEngineResult(pageView, this);
            }
            else
            {
                return new ViewEngineResult(new[] { templateFilename });
            }
        }

        PageView CreateAndCachePageView(ControllerContext controllerContext, string viewName, string templateFilename)
        {
            var html = XDocument.Load(templateFilename);
            if (html.Root.Name.LocalName == "use-master")
            {
                var masterFilename = Path.Combine(websiteConfiguration.TemplateDirectory, html.Root.Attribute("id").Value);
                var pageView = CreatePageViewWithMaster(html, masterFilename);
                controllerContext.HttpContext.Cache.Insert(viewName, pageView, new CacheDependency(new[] { masterFilename, templateFilename }));
                return pageView;
            }
            else
            {
                var pageView = new PageView(html, getPlugin);
                controllerContext.HttpContext.Cache.Insert(viewName, pageView, new CacheDependency(templateFilename));
                return pageView;
            }
        }

        PageView CreatePageViewWithMaster(XDocument contentHtml, string masterFilename)
        {
            var masterHtml = XDocument.Load(masterFilename);
            var placeholders = masterHtml.DescendantNodes().OfType<XProcessingInstruction>().Where(pi => pi.Target == "placeholder").ToArray();
            foreach (var placeholder in placeholders)
            {
                var placeholderId = placeholder.Data;
                var content = contentHtml.Root.Elements().FirstOrDefault(e => e.Attribute("for").Value == placeholderId);
                if (content == null)
                {
                    placeholder.Remove();
                }
                else
                {
                    placeholder.ReplaceWith(content.Nodes());
                }
            }
            return new PageView(masterHtml, getPlugin);
        }

        static PageView GetPageViewFromCache(ControllerContext controllerContext, string viewName)
        {
            return controllerContext.HttpContext.Cache.Get(viewName) as PageView;
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
        }
    }
}
