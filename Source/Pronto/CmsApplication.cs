using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using Pronto.Authorization;
using Pronto.Controllers;
using Pronto.Views;

namespace Pronto
{
    public class CmsApplication : HttpApplication, IContainerProviderAccessor
    {
        internal static bool IsConfigured = false;

        protected virtual void Application_Start()
        {
            AssignApplicationIsConfigured();

            BuildContainer();
            RegisterRoutes(RouteTable.Routes);
            RegisterViewEngine();
        }

        void AssignApplicationIsConfigured()
        {
            IsConfigured = !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["auth-type"]);
        }

        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!IsConfigured && !Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/_config"))
            {
                Response.Redirect("~/_config");
            }
        }

        protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User == null || HttpContext.Current.User.Identity == null) return;

            if (AuthType == "OpenId")
            {
                AuthorizeOpenId();
            }
            else if (AuthType == "SimplePassword")
            {
                AuthorizeSimplePassword();
            }
        }

        protected virtual void Application_EndRequest()
        {
            containerProvider.DisposeRequestContainer();
        }
        
        void AuthorizeOpenId()
        {
            var service = ContainerProvider.ApplicationContainer.Resolve<IResourceService<Authorizer, IReadOnlyAuthorizer>>();
            using (var reader = service.CreateReader())
            {
                var isAdmin = reader.Resource.IsUserAdmin(HttpContext.Current.User.Identity.Name);
                if (isAdmin)
                {
                    HttpContext.Current.User = new GenericPrincipal(
                        HttpContext.Current.User.Identity,
                        new[] { "admin" }
                    );
                }
            }
        }

        static void AuthorizeSimplePassword()
        {
            if (HttpContext.Current.User.Identity.Name == "admin")
            {
                HttpContext.Current.User = new GenericPrincipal(
                    HttpContext.Current.User.Identity,
                    new[] { "admin" }
                );
            }
        }

        protected virtual void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            if (IsConfigured)
            {
                routes.MapRoute(
                    "Plugin",
                    "_plugins/{controller}/{action}/{id}",
                    new { id = "" },
                    new[] { typeof(ContentController).Namespace });

                routes.MapRoute(
                    "Content",
                    "_content/{action}/{id}",
                    new { controller = "Content", id = "" },
                    new[] { typeof(ContentController).Namespace });

                routes.MapRoute(
                    "PageAdmin",
                    "_page/{action}/{*path}",
                    new { controller = "Page", path = "" },
                    new[] { typeof(PageController).Namespace });

                routes.MapRoute(
                    "Theme",
                    "_theme/{*path}",
                    new { controller = "Theme", action = "FileAction" },
                    new[] { typeof(ThemeController).Namespace });

                routes.MapRoute(
                    "Auth",
                    "_auth/{action}",
                    new { controller = AuthType },
                    new[] { typeof(PageController).Namespace });

                routes.MapRoute(
                    "Page",
                    "{*path}",
                    new { controller = "Page", action = "GetPage", path = "" },
                    new[] { typeof(PageController).Namespace }
                );
            }
            else
            {
                routes.MapRoute(
                    "Config",
                    "_config/{action}",
                    new { controller = "Configuration", action = "Index" },
                    new[] { typeof(ConfigurationController).Namespace });
            }
        }

        public static string AuthType
        {
            get
            {
                return WebConfigurationManager.AppSettings["auth-type"];
            }
        }

        void BuildContainer()
        {
            var builder = new ContainerBuilder();
            RegisterContainerComponents(builder);

            containerProvider = new ContainerProvider(builder.Build());
            ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(containerProvider));
        }

        protected virtual void RegisterContainerComponents(ContainerBuilder builder)
        {
            RegisterControllers(builder);
            builder.Register(c => new HttpContextWrapper(HttpContext.Current)).As<HttpContextBase>();
            builder.Register(c => c.Resolve<HttpContextBase>().Server);
            builder.Register(c => c.Resolve<HttpContextBase>().Request);
            builder.Register(c => c.Resolve<HttpContextBase>().Response);
            builder.Register(c => HttpContext.Current.Cache).HttpRequestScoped();
            builder.Register(CreateWebsiteConfiguration());           
            builder.Register<WebsiteService>().As<IWebsiteService>().HttpRequestScoped();

            var authorizationFilename = Server.MapPath("~/app_data/authorization.xml");
            builder.Register(c => new AuthorizerService(authorizationFilename, c.Resolve<Cache>()))
                   .As<IResourceService<Authorizer, IReadOnlyAuthorizer>>().HttpRequestScoped();
            builder.Register<SimplePasswordService>();

            RegisterPagePlugins(builder);

            builder.Register<PageViewEngine>();
        }

        static void RegisterControllers(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacControllerModule(
                BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray()
            ));
        }

        protected virtual WebsiteConfiguration CreateWebsiteConfiguration()
        {
            return new WebsiteConfiguration(
                Server.MapPath("~/app_data/website.xml"),
                WebConfigurationManager.AppSettings["theme"],
                Server.MapPath("~/templates"),
                ""
            );
        }

        void RegisterPagePlugins(ContainerBuilder builder)
        {
            foreach (var type in FindPagePluginTypes())
            {
                builder.Register(type)
                    .Named(GetPagePluginName(type))
                    .FactoryScoped();
            }
            // Factory function that creates plugins by name.
            builder.Register(c => new Func<string, IPagePlugin>(name => (IPagePlugin)c.Resolve(new NamedService(name))));
        }
                
        protected virtual string GetPagePluginName(Type type)
        {
            if (type.Name.EndsWith("plugin", StringComparison.OrdinalIgnoreCase))
            {
                return type.Name.Substring(0, type.Name.Length - "plugin".Length).ToLowerInvariant();
            }
            else
            {
                return type.Name.ToLowerInvariant();
            }
        }

        protected virtual IEnumerable<Type> FindPagePluginTypes()
        {
            return from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                   from type in assembly.GetExportedTypes()
                   where IsPlugin(type)
                   select type;
        }

        protected virtual bool IsPlugin(Type type)
        {
            return type.IsClass 
                && !type.IsAbstract 
                && typeof(IPagePlugin).IsAssignableFrom(type);
        }
        
        void RegisterViewEngine()
        {
            ViewEngines.Engines.Clear();

            var viewEngine = ContainerProvider.ApplicationContainer.Resolve<PageViewEngine>();
            ViewEngines.Engines.Add(viewEngine);
        }
    
        #region IContainerProviderAccessor Members

        static IContainerProvider containerProvider;

        public IContainerProvider ContainerProvider
        {
            get { return containerProvider; }
        }

        #endregion
    }
}
