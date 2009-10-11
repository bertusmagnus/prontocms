using System.Web.Caching;
using System.Xml.Linq;

namespace Pronto.Authorization
{
    class AuthorizerService : FileBasedResourceService<Authorizer, IReadOnlyAuthorizer>
    {
        public AuthorizerService(string authorizationXmlFilename, Cache cache)
            : base(authorizationXmlFilename, cache)
        {
        }

        protected override Authorizer LoadResource(string filename)
        {
            var doc = XDocument.Load(filename);
            return new Authorizer(doc.Root);
        }

        protected override void SaveResource(Authorizer resource, string filename)
        {
            resource.Save(filename);
        }
    }
}
