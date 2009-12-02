using System;
using System.Collections.Generic;

namespace Pronto
{
    public interface IReadOnlyWebsite : IEnumerable<IReadOnlyPage>
    {
        IReadOnlyPage FindPage(string path);
        IReadOnlyPage FindCurrentPageAtLevel(int level, IReadOnlyPage currentPage);
        string GetContent(string id);
        string Title { get; }
    }
}
