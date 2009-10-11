using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace Pronto.Controllers
{
    public class SimplePasswordController : Controller
    {
        readonly string PasswordSettingKey = "password";

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/");
        }

        public ActionResult LogIn(string password)
        {
            var expectedPassword = WebConfigurationManager.AppSettings[PasswordSettingKey];
            if (expectedPassword == null)
                throw new Exception(PasswordSettingKey + " must be set in web.config appSettings.");

            if (password == expectedPassword)
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

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangePassword(string password, string newPassword, string newPasswordRepeat)
        {
            if (password != WebConfigurationManager.AppSettings[PasswordSettingKey])
            {
                Response.StatusCode = 400;
                return Content("Current password incorrect.", "text/plain");
            }
            if (newPassword != newPasswordRepeat)
            {
                Response.StatusCode = 400;
                return Content("New password does not match the repeat.", "text/plain");
            }

            var config = WebConfigurationManager.OpenWebConfiguration("/");
            config.AppSettings.Settings[PasswordSettingKey].Value = newPassword;
            config.Save();

            return Content("Password changed.", "text/plain");
        }
    }
}
