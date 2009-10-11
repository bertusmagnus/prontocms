using System;
using System.Linq;
using System.Web.Mvc;
using Pronto.Controllers;

namespace Pronto.Authorization
{
    [HandleErrorForAjaxRequest]
    public class AuthorizationController : Controller
    {
        public AuthorizationController(IResourceService<Authorizer, IReadOnlyAuthorizer> service)
        {
            this.service = service;
        }

        IResourceService<Authorizer, IReadOnlyAuthorizer> service;

        [AuthorizeAdmin]
        public ActionResult ListAdmins()
        {
            Response.Cache.SetNoStore();
            using (var reader = service.CreateReader())
            {
                return Json(reader.Resource.Admins.ToArray());
            }
        }

        [AuthorizeAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateToken()
        {
            using (var writer = service.CreateWriter())
            {
                return Content(writer.Resource.CreateToken(), "text/plain");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddAdmin(string name, string token)
        {
            using (var writer = service.CreateWriter())
            {
                writer.Resource.AddAdmin(User.Identity.Name, name, token);
            }
            return Content("OK", "text/plain");
        }

        [AuthorizeAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteAdmin(string id)
        {
            if (User.Identity.Name == id)
            {
                Response.StatusCode = 400;
                return Content("You cannot delete yourself.", "text/plain");
            }
            else
            {
                using (var writer = service.CreateWriter())
                {
                    writer.Resource.DeleteAdmin(id);
                }
                return null;
            }
        }
    }
}
