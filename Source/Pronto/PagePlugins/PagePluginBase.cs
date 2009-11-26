using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Pronto.PagePlugins
{
    public abstract class PagePluginBase : IPagePlugin
    {
        public ViewContext ViewContext { get; private set; }
        public IReadOnlyPage Page { get; private set; }
        public IReadOnlyWebsite Website { get; private set; }
        public UrlHelper Url { get; private set; }

        public virtual void Initialize(ViewContext viewContext)
        {
            ViewContext = viewContext;
            Page = (IReadOnlyPage)viewContext.ViewData["page"];
            Website = (IReadOnlyWebsite)viewContext.ViewData["website"];
            Url = new UrlHelper(viewContext.RequestContext);
        }

        public virtual IEnumerable<XObject> GetHeadContents(bool firstUse)
        {
            return Enumerable.Empty<XObject>();
        }

        public abstract IEnumerable<XObject> Render(string data);

        public virtual IEnumerable<XElement> GetScripts(bool firstUse)
        {
            return Enumerable.Empty<XElement>();
        }

        /// <summary>
        /// Is the current user in the role "admin".
        /// </summary>
        protected bool IsUserAdmin
        {
            get { return ViewContext.HttpContext.User.IsInRole("admin"); }
        }

        /// <summary>
        /// Application path including the trailing forward slash.
        /// </summary>
        protected string UrlBase
        {
            get { return ViewContext.HttpContext.Request.ApplicationPath.TrimEnd('/') + "/"; }
        }

    }
}
