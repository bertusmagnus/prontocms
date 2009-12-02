using System.Web.Mvc;
using System.Web.Security;
using Pronto.Authorization;

namespace Pronto.Controllers
{
    public class SimplePasswordController : Controller
    {
        public SimplePasswordController(SimplePasswordService service)
        {
            this.service = service;
        }

        readonly SimplePasswordService service;
        readonly string PasswordSettingKey = "password";

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/");
        }

        public ActionResult LogIn(string password)
        {
            using (var reader = service.CreateReader())
            {
                if (reader.Resource.IsCorrect(password))
                {
                    FormsAuthentication.SetAuthCookie("admin", false);
                    return Content("OK", "text/plain");
                }
                else
                {
                    Response.StatusCode = 400;
                    return Content("Incorrect password.", "text/plain");
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangePassword(string password, string newPassword, string newPasswordRepeat)
        {
            using (var writer = service.CreateWriter())
            {
                if (!writer.Resource.IsCorrect(password))
                {
                    Response.StatusCode = 400;
                    return Content("Current password incorrect.", "text/plain");
                }
                if (newPassword != newPasswordRepeat)
                {
                    Response.StatusCode = 400;
                    return Content("New password does not match the repeat.", "text/plain");
                }

                writer.Resource.ChangePassword(newPassword);

                return Content("Password changed.", "text/plain");
            }
        }
    }
}
