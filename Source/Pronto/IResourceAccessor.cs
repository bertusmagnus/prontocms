using System;

namespace Pronto
{
    public interface IResourceAccessor<T> : IDisposable
        where T : class
    {
        T Resource { get; }
    }
}
