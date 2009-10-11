using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Xunit;

namespace Pronto.Views
{
    public class PageViewTests
    {
        [Fact]
        public void Adds_description_meta_tag()
        {
            var html = XDocument.Parse("<html><head><title>Test</title></head><body></body></html>");
            var pageview = new PageView(html, s => null);
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var viewContext = FakeViewContext(pageview);
                viewContext.ViewData["page"] = new Page(new FakePageContainer(), "", "", "", false, "test");
                pageview.Render(viewContext, writer);
            }
            Assert.Contains("<meta name=\"description\" content=\"test\" />", sb.ToString());
        }

        static ViewContext FakeViewContext(PageView pageview)
        {
            return new ViewContext(
                new ControllerContext(
                    new RequestContext(
                        new FakeHttpContext(), 
                        new RouteData()
                    ), 
                    new FakeController()
                ), 
                pageview, 
                new ViewDataDictionary(), 
                new TempDataDictionary()
            );
        }
    }

    class FakeHttpContext : HttpContextBase
    {
        public override bool IsDebuggingEnabled
        {
            get { return false; }
        }
    }

    class FakeController : Controller { }
}
