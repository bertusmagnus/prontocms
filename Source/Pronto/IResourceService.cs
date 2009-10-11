using System;

namespace Pronto
{
    public interface IResourceService<T, TReadOnly>
        where T : class, TReadOnly
        where TReadOnly : class
    {
        IResourceAccessor<TReadOnly> CreateReader();
        IResourceAccessor<T> CreateWriter();
        void Replace(Func<T, T> replacer);
    }
}
