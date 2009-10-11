using System;
using System.Threading;

namespace Pronto
{
    class LockManager : IDisposable
    {
        enum LockTypes { None, Read, Write, Upgradeable }

        ReaderWriterLockSlim readerWriterLock;
        LockTypes enteredLockType = LockTypes.None;

        public LockManager(ReaderWriterLockSlim readerWriterLock)
        {
            this.readerWriterLock = readerWriterLock;
        }

        public void EnterReadLock()
        {
            readerWriterLock.EnterReadLock();
            enteredLockType = LockTypes.Read;
        }

        public void EnterWriteLock()
        {
            readerWriterLock.EnterWriteLock();
            enteredLockType = LockTypes.Write;
        }

        public void EnterUpgradeableReadLock()
        {
            readerWriterLock.EnterUpgradeableReadLock();
            enteredLockType = LockTypes.Upgradeable;
        }

        public void DowngradeToRead()
        {
            if (enteredLockType != LockTypes.Upgradeable) 
                throw new InvalidOperationException("Upgradeable read lock is not held.");

            readerWriterLock.EnterReadLock();
            readerWriterLock.ExitUpgradeableReadLock();
            enteredLockType = LockTypes.Read;
        }

        public void ExitLock()
        {
            switch (enteredLockType)
            {
                case LockTypes.Read:
                    readerWriterLock.ExitReadLock();
                    break;
                case LockTypes.Write:
                    readerWriterLock.ExitWriteLock();
                    break;
                case LockTypes.Upgradeable:
                    readerWriterLock.ExitUpgradeableReadLock();
                    break;
                default: 
                    throw new InvalidOperationException("Lock not held.");
            }
        }

        public void Dispose()
        {
            if (readerWriterLock != null)
            {
                ExitLock();
                readerWriterLock = null;
            }
        }

        ~LockManager()
        {
            Dispose();
        }
    }
}
