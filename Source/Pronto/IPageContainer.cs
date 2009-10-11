using System.Collections.Generic;

namespace Pronto
{
    public interface IPageContainer
    {
        string Path { get; }
        List<Page> Pages { get; }
    }
}
