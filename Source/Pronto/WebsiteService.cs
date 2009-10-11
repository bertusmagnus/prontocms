using System.Web.Caching;
using System.Xml.Linq;

namespace Pronto
{
    public class WebsiteService : FileBasedResourceService<Website, IReadOnlyWebsite>, IWebsiteService
    {
        public WebsiteService(WebsiteConfiguration websiteConfiguration, Cache cache)
            : base(websiteConfiguration.WebsiteXmlFilename, cache)
        {
        }

        protected override Website LoadResource(string filename)
        {
            return new Website(XDocument.Load(filename).Root);
        }

        protected override void SaveResource(Website resource, string filename)
        {
            resource.Save(filename);
        }
    }
}