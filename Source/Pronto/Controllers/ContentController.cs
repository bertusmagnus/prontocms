using System.Web.Mvc;

namespace Pronto.Controllers
{
    [AuthorizeAdmin]
    [HandleErrorForAjaxRequest]
    public class ContentController : Controller
    {
        public ContentController(IWebsiteService websiteService)
        {
            this.websiteService = websiteService;
        }

        IWebsiteService websiteService;

        public ActionResult GetContent(string id, string path)
        {
            using (var reader = websiteService.CreateReader())
            {
                var content = reader.Resource.FindPage(path).GetContent(id);
                return Content(content, "text/plain");
            } 
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public void SaveContent(string id, string path, string content)
        {
            websiteService.UpdatePage(path,
                p => p.SetContent(id, content)
            );
        }

        public ActionResult GetGlobalContent(string id)
        {
            using (var reader = websiteService.CreateReader())
            {
                var content = reader.Resource.GetContent(id);
                return Content(content, "text/plain");
            }            
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public void SaveGlobalContent(string id, string content)
        {
            using (var writer = websiteService.CreateWriter())
            {
                writer.Resource.SetContent(id, content);
            }
        }
    }
}
