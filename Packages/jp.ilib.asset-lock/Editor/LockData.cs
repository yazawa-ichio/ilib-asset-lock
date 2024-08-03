using System;

namespace ILib.AssetLock
{
	public enum LockStatus
	{
		None = 0,
		Lock = 1,
		Unlock = 2,
	}

	[Serializable]
	public class LockData
	{
		public static readonly LockData Empty = new LockData()
		{
			Status = LockStatus.Lock,
		};

		public int Id;

		public string Path;

		public string User;

		public LockStatus Status = LockStatus.None;

		public bool EditMode;

		public string GitHash;

		public AssetLockTarget TargetData;

		public bool IsMyLock()
		{
			return User == Git.User();
		}
	}
}
