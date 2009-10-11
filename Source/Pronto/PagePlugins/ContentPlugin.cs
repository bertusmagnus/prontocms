using System.Web.Mvc;

namespace Pronto.PagePlugins
{
    public class ContentPlugin : ContentPluginBase
    {
        protected override string GetContent(string contentId)
        {
            return Page.GetContent(contentId);
        }
    }
}
