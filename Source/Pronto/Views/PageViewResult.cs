using System.Web.Mvc;

namespace Pronto.Views
{
    class PageViewResult : ViewResult
    {
        public PageViewResult(string path, IResourceService<Website, IReadOnlyWebsite> websiteService)
        {
            this.path = path;
            this.websiteService = websiteService;
        }

        IResourceService<Website, IReadOnlyWebsite> websiteService;
        string path;

        public override void ExecuteResult(ControllerContext context)
        {
            using (var reader = websiteService.CreateReader())
            {
                var page = reader.Resource.FindPage(path);
                if (page != null)
                {
                    ViewName = page.Template;
                    ViewData["page"] = page;
                    ViewData["website"] = reader.Resource;
                    base.ExecuteResult(context);
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 404;
                }
            }
        }
    }

}
