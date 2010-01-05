using System.Web.Mvc;

namespace Pronto.PagePlugins
{
    public class GlobalContentPlugin : ContentPluginBase
    {
        public GlobalContentPlugin(WebsiteConfiguration config)
            : base(config)
        {
        }

        protected override string GetContent(string contentId)
        {
            return Website.GetContent(contentId);
        }

        protected override string EditableCssClass
        {
            get
            {
                return "global-editable";
            }
        }
    }
}
