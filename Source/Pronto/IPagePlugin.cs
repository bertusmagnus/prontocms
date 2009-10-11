using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Pronto
{
    public interface IPagePlugin
    {
        void Initialize(ViewContext context);
        IEnumerable<XObject> GetHeadContents(bool firstUse);
        IEnumerable<XObject> Render(string data);
    }
}
