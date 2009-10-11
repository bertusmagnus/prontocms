using System;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;

namespace Pronto.Controllers
{
    public class OpenIdController : Controller
    {
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Login()
        {
            var openid = new OpenIdRelyingParty();
            if (openid.Response == null)
            {
                throw new InvalidOperationException("Not an OpenID response.");
            }
            else
            {
                var returnUrl = (string)Session["returnUrl"] ?? "~/";
                // Stage 3: OpenID Provider sending assertion response
                switch (openid.Response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        FormsAuthentication.SetAuthCookie(openid.Response.ClaimedIdentifier, false);
                        return Redirect(returnUrl);

                    case AuthenticationStatus.Canceled:
                        return Redirect(returnUrl + "#login-cancelled");
                    default:
                        return Redirect(returnUrl + "#login-fail");
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(string id, string returnUrl)
        {
            var openid = new OpenIdRelyingParty();
            // Stage 2: user submitting Identifier
            Identifier identifier;
            if (!string.IsNullOrEmpty(id) && Identifier.TryParse(id, out identifier))
            {
                try
                {
                    var req = openid.CreateRequest(identifier);
                    var location = req.RedirectingResponse.Headers["Location"];
                    Session["returnUrl"] = returnUrl;
                    return Content(location, "text/plain");
                }
                catch (Exception)
                {
                }
            }

            Response.StatusCode = 400;
            return Content("Cannot login, check your OpenID is correct.");
        }
    }
}
