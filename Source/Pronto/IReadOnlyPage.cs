using System;
using System.Collections.Generic;

namespace Pronto
{
    public interface IReadOnlyPage : IEnumerable<IReadOnlyPage>
    {
        IReadOnlyPage FindPage(IEnumerable<string> names);
        bool Contains(IReadOnlyPage page);
        string GetContent(string id);
        string Name { get; }
        bool Navigation { get; }
        string NavigationText { get; }
        string Path { get; }
        string Template { get; }
        string Title { get; }
        string Description { get; }
    }
}
