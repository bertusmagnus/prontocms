using System.Xml.Linq;

namespace Pronto.Views
{
    public interface IPageViewModifier
    {
        void Modify(XDocument html, IReadOnlyPage page);
    }
}
