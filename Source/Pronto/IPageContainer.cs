﻿using System.Collections.Generic;

namespace Pronto
{
    public interface IPageContainer
    {
        string Path { get; }
        List<Page> Pages { get; }
        bool Contains(IReadOnlyPage page);
        int Level { get; }
    }
}
