using System;

namespace ILib.AssetLock
{
	public interface ILockDB
	{
		LockData GetData(AssetLockTarget target, string path);
		void TryLock(AssetLockTarget target, string path, Action onLock = null);
		void Unlock(AssetLockTarget target, string path);
		void OnGUI();
	}

	public static class ILockDBExtension
	{
		public static bool TryGetLockData(this ILockDB self, string path, out LockData data)
		{
			data = null;
			var db = self;
			if (db == null)
			{
				return false;
			}
			if (path.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			var settings = AssetLockSettings.I;
			foreach (var target in settings.Targets)
			{
				if (target.Regex.IsMatch(path))
				{
					data = db.GetData(target, path);
					return true;
				}
			}
			return false;
		}

		public static void TryLock(this ILockDB self, string path, Action onLock = null)
		{
			var db = self;
			if (db == null)
			{
				return;
			}
			var settings = AssetLockSettings.I;
			foreach (var target in settings.Targets)
			{
				if (target.Regex.IsMatch(path))
				{
					self?.TryLock(target, path, onLock);
					return;
				}
			}
		}

		public static void Unlock(this ILockDB self, string path)
		{
			var db = self;
			if (db == null)
			{
				return;
			}
			var settings = AssetLockSettings.I;
			foreach (var target in settings.Targets)
			{
				if (target.Regex.IsMatch(path))
				{
					self?.Unlock(target, path);
					return;
				}
			}
		}

		public static bool CanEdit(this ILockDB self, string path)
		{
			if (self.TryGetLockData(path, out var data))
			{
				return data.IsMyLock();
			}
			return true;
		}

		public static bool IsLocked(this ILockDB self, string path)
		{
			if (self.TryGetLockData(path, out var data))
			{
				if (data.IsMyLock())
				{
					return false;
				}
				return data.Status == LockStatus.Lock;
			}
			return false;
		}

	}
}