using System;
using System.Collections.Generic;

namespace Pronto
{
    public interface IReadOnlyWebsite : IEnumerable<IReadOnlyPage>
    {
        IReadOnlyPage FindPage(string path);
        string GetContent(string id);
        string Title { get; }
    }
}
