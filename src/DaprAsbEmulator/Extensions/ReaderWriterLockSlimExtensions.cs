using System.Reactive.Disposables;

namespace DaprAsbEmulator.Extensions;

public static class ReaderWriterLockSlimExtensions
{
    public static IDisposable ReadLock(this ReaderWriterLockSlim rwLock)
    {
        rwLock.EnterReadLock();
        return Disposable.Create(rwLock, x => x.ExitReadLock());
    }

    public static IDisposable WriteLock(this ReaderWriterLockSlim rwLock)
    {
        rwLock.EnterWriteLock();
        return Disposable.Create(rwLock, x => x.ExitWriteLock());
    }

    public static IDisposable UpgradableReadLock(this ReaderWriterLockSlim rwLock)
    {
        rwLock.EnterUpgradeableReadLock();
        return Disposable.Create(rwLock, x => x.ExitUpgradeableReadLock());
    }
}