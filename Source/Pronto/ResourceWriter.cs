using System;
using System.Threading;

namespace Pronto
{
    public class ResourceWriter<T> : IResourceAccessor<T>, IDisposable
        where T : class
    {
        public ResourceWriter(Func<T> resource, ReaderWriterLockSlim resourceLock, Action<T> save)
        {
            try
            {
                manager = new LockManager(resourceLock);
                manager.EnterWriteLock();
                Resource = resource();
                this.save = save;
            }
            catch
            {
                manager.Dispose();
                throw;
            }
        }

        LockManager manager;
        Action<T> save;
        public T Resource { get; private set; }

        void IDisposable.Dispose()
        {
            try
            {
                save(Resource);
            }
            finally
            {
                manager.Dispose();
                manager = null;
                Resource = null;
            }
        }
    }
}
