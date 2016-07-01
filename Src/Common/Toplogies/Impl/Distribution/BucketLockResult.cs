namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution
{
    #region /                 --- BucketLockResult ---           /

    public enum BucketLockResult
    {
        LockAcquired,
        OwnerChanged,
        AlreadyLocked
    }

    #endregion
}
