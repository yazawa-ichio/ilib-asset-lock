#if ILIB_ASSET_LOCK_TEST
using System;
using System.Collections.Generic;
using UnityEditor;

namespace ILib.AssetLock
{
	public class TestLockDB : ILockDB
	{
		class CategoryData
		{
			public string Category;

			public Dictionary<string, LockData> Data = new Dictionary<string, LockData>();
		}

		Dictionary<string, CategoryData> m_Dic = new Dictionary<string, CategoryData>();

		LockData Get(AssetLockTarget target, string path)
		{
			if (!m_Dic.TryGetValue(target.Category, out var data))
			{
				data = m_Dic[target.Category] = new CategoryData();
			}
			if (!data.Data.TryGetValue(path, out var lockData))
			{
				lockData = data.Data[path] = new LockData()
				{
					Path = path,
					TargetData = target,
				};
			}
			return lockData;
		}

		public LockData GetData(AssetLockTarget target, string path)
		{
			return Get(target, path);
		}

		public void TryLock(AssetLockTarget target, string path, Action onLock)
		{
			var data = GetData(target, path);
			if (data.Status == LockStatus.Lock)
			{
				if (!EditorUtility.DisplayDialog("TryLock", "Force Lock", "OK"))
				{
					return;
				}
			}
			data.Status = LockStatus.Unlock;
			data.User = Git.User();
			onLock?.Invoke();
		}

		public void Unlock(AssetLockTarget target, string path)
		{
			var data = GetData(target, path);
			data.Status = LockStatus.Unlock;
			data.GitHash = Git.GetCurrentHash();
			data.User = null;
		}

		public void OnGUI() { }
	}
}
#endif