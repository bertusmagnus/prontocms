using System;
using System.Threading;

namespace Pronto
{
    class ResourceReader<T> : IDisposable, IResourceAccessor<T>
        where T : class
    {
        public ResourceReader(Func<T> resource, ReaderWriterLockSlim resourceLock)
        {
            try
            {
                manager = new LockManager(resourceLock);
                manager.EnterUpgradeableReadLock();
                Resource = resource();
                manager.DowngradeToRead();
            }
            catch
            {
                manager.Dispose();
                throw;
            }
        }

        LockManager manager;
        public T Resource { get; private set; }

        void IDisposable.Dispose()
        {
            manager.Dispose();
        }
    }
}
