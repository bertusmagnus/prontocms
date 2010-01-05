using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Pronto.Views;

namespace Pronto.Controllers
{
    [HandleErrorForAjaxRequest]
    public class PageController : Controller
    {
        public PageController(IWebsiteService websiteService)
        {
            this.websiteService = websiteService;
        }

        IWebsiteService websiteService;

        public ActionResult GetPage(string path)
        {
            path = TransformPath(path);
            return new PageViewResult(path, websiteService);
        }

        [AuthorizeAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public void Update(string path, string newTitle, string name, string template, bool? navigation, string description)
        {
            websiteService.UpdatePage(path,
                page =>
                {
                    page.Title = newTitle;
                    page.Name = name;
                    page.Template = template;
                    page.Navigation = navigation.GetValueOrDefault();
                    page.Description = description;
                }
            );
        }

        [AuthorizeAdmin]
        [ValidateInput(false)]
        public void ChangeTitle(string path, string newTitle)
        {
            websiteService.UpdatePage(path,
                page => { page.Title = newTitle; }
            );
        }

        [AuthorizeAdmin]
        public void ChangeName(string path, string name)
        {
            websiteService.UpdatePage(path,
                page => { page.Name = name; }
            );
        }

        [AuthorizeAdmin]
        public void SetTemplate(string path, string template)
        {
            websiteService.UpdatePage(path,
                page => { page.Template = template; }
            );
        }

        [AuthorizeAdmin]
        public void SetNavigation(string path, bool? navigation)
        {
            websiteService.UpdatePage(path,
                page => { page.Navigation = navigation.GetValueOrDefault(); }
            );
        }

        [AuthorizeAdmin]
        public void Delete(string path)
        {
            websiteService.UpdatePage(path,
                page => { page.Delete(); }
            );
        }

        [AuthorizeAdmin]
        [ValidateInput(false)]
        public ActionResult Add(string newTitle, string template, bool? navigation, string description, string referrerPath, string mode)
        {
            Page newPage = null;
            websiteService.UpdatePage(referrerPath,
                referrer => 
                {
                    if (mode == "child")
                    {
                        newPage = referrer.AddNewChildPage(newTitle, template, navigation.GetValueOrDefault(), description ?? "");
                    }
                    else
                    {
                        newPage = referrer.AddNewSiblingPage(newTitle, template, navigation.GetValueOrDefault(), description ?? "");
                    }
                }
            );
            return Content(newPage.Path, "text/plain");
        }

        [AuthorizeAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult ReorganisePages(string referrerPath)
        {
            var xml = ReadXDocumentFromRequest();
            string movedPath = referrerPath;
            websiteService.Replace(oldWebsite => oldWebsite.Reorganise(xml, ref movedPath));
            return Content(movedPath, "text/plain");
        }

        protected virtual string TransformPath(string path)
        {
            return path;
        }

        XDocument ReadXDocumentFromRequest()
        {
            using (var reader = new StreamReader(Request.InputStream))
            {
                return XDocument.Parse(reader.ReadToEnd());
            }
        }

    }
}
