using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Pronto.Authorization;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using System.Configuration;
using DotNetOpenId.RelyingParty;
using DotNetOpenId;
using System.Web.Security;

namespace Pronto.Controllers
{
    [ConfigurationControllerAuthorization]
    public class ConfigurationController : Controller
    {
        public ConfigurationController(IWebsiteService websiteService, IResourceService<Authorizer, IReadOnlyAuthorizer> authService)
        {
            this.websiteService = websiteService;
            this.authService = authService;
        }

        IWebsiteService websiteService;
        IResourceService<Authorizer, IReadOnlyAuthorizer> authService;

        public ActionResult Index()
        {
            var html = GetSetupHtml();
            html = html.Replace("$path", HttpContext.Request.ApplicationPath.TrimEnd('/') + "/");
            return Content(html, "text/html");
        }

        public void SetWebsiteTitle(string title)
        {
            using (var writer = websiteService.CreateWriter())
            {
                writer.Resource.Title = title;
            }
        }

        public void SetSimplePassword(string password)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");

            SetConfig(config, "password", password);
            SetConfig(config, "auth-type", "SimplePassword");
            config.Save();

            CmsApplication.IsConfigured = true;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void Login(string id)
        {
            var openid = new OpenIdRelyingParty();
            Identifier identifier;
            if (!string.IsNullOrEmpty(id) && Identifier.TryParse(id, out identifier))
            {
                openid.CreateRequest(
                    identifier, 
                    new Realm(Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath),
                    new Uri(Url.RouteUrl("Config", new { action = "Login" }, "http"))
                ).RedirectToProvider();
            }
            else
            {
                throw new ArgumentException("Invalid OpenID.");
            }
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
                switch (openid.Response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        SetOpenId(openid.Response.ClaimedIdentifier);
                        FormsAuthentication.SetAuthCookie(openid.Response.ClaimedIdentifier, false);
                        return Redirect("~");

                    default:
                        return Redirect("~");
                }
            }
        }

        string GetSetupHtml()
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(CmsApplication), "setup.htm")))
            {
                return reader.ReadToEnd();
            }
        }

        void SetOpenId(string openId)
        {
            using (var writer = authService.CreateWriter())
            {
                writer.Resource.AddAdmin(openId, "Admin");
            }

            var config = WebConfigurationManager.OpenWebConfiguration("~");
            SetConfig(config, "auth-type", "OpenId");
            config.Save();

            CmsApplication.IsConfigured = true;
        }

        void SetConfig(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
        }
    }

    public class ConfigurationControllerAuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (CmsApplication.IsConfigured)
            {
                filterContext.Result = new RedirectResult("~/");
            }
        }
    }
}
