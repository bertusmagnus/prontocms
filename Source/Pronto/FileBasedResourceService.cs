using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Caching;

namespace Pronto
{
    /// <summary>
    /// Provides thread-safe, read and write access to a cached resource, which is loaded from a file.
    /// </summary>
    /// <typeparam name="T">The type of the resource</typeparam>
    public abstract class FileBasedResourceService<T, TReadOnly> : IResourceService<T, TReadOnly>
        where T : class, TReadOnly
        where TReadOnly : class
    {
        string filename;
        Cache cache;
        static ReaderWriterLockSlim resourceLock = new ReaderWriterLockSlim();

        public FileBasedResourceService(string filename, Cache cache)
        {
            this.filename = filename;
            this.cache = cache;
        }

        public IResourceAccessor<TReadOnly> CreateReader()
        {
            return new ResourceReader<TReadOnly>(GetResource, resourceLock);
        }

        public IResourceAccessor<T> CreateWriter()
        {
            return new ResourceWriter<T>(GetResource, resourceLock, r => SaveResource(r, filename));
        }

        public void Replace(Func<T, T> replacer)
        {
            using (var manager = new LockManager(resourceLock))
            {
                manager.EnterWriteLock();
                var resource = GetResource();
                var newResource = replacer(resource);
                SaveResource(newResource, filename);
            }
        }

        protected abstract T LoadResource(string filename);
        protected abstract void SaveResource(T resource, string filename);

        T GetResource()
        {
            Debug.Assert(resourceLock.IsUpgradeableReadLockHeld || resourceLock.IsWriteLockHeld);

            var resource = GetResourceFromCache();
            if (resource != null)
            {
                return resource;
            }
            else
            {
                return GetAndCacheResource();
            }
        }

        T GetResourceFromCache()
        {
            return cache.Get(CacheKey()) as T;
        }

        T GetAndCacheResource()
        {
            if (resourceLock.IsWriteLockHeld)
            {
                T resource = LoadResource(filename);
                cache.Insert(CacheKey(), resource, new CacheDependency(filename));
                return resource;
            }
            else
            {
                resourceLock.EnterWriteLock();
                try
                {
                    T resource = GetResourceFromCache() ?? LoadResource(filename);
                    cache.Insert(CacheKey(), resource, new CacheDependency(filename));
                    return resource;
                }
                finally
                {
                    resourceLock.ExitWriteLock();
                }
            }
        }

        protected virtual string CacheKey()
        {
            return typeof(T).FullName;
        }
    }
}
