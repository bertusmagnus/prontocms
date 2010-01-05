using System.Web.Mvc;

namespace Pronto.PagePlugins
{
    public class ContentPlugin : ContentPluginBase
    {
        public ContentPlugin(WebsiteConfiguration config)
            : base(config)
        {
        }

        protected override string GetContent(string contentId)
        {
            return Page.GetContent(contentId);
        }
    }
}
