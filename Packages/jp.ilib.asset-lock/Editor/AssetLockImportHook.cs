using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Pool;

namespace ILib.AssetLock
{

	public class AssetLockImportHook : AssetModificationProcessor
	{
		static ILockDB DB => AssetLockSettings.I.LockDB;

		[OnOpenAsset(0)]
		static bool OnOpen(int instanceID, int line)
		{
			var path = AssetDatabase.GetAssetPath(instanceID);
			if (DB.TryGetLockData(path, out var data))
			{
				if (data.IsMyLock())
				{
					return false;
				}
				else
				{
					if (data.Status == LockStatus.Lock)
					{
						EditorUtility.DisplayDialog("AssetLock", $"Asset Locking {data.User}", "OK");
					}
					else
					{
						if (EditorUtility.DisplayDialog("AssetLock", $"Asset Lock?", "YES", "NO"))
						{
							DB.TryLock(path, () =>
							{
								if (DB.CanEdit(path))
								{
									AssetDatabase.OpenAsset(instanceID, line);
								}
							});
							return true;
						}
					}
					return true;
				}
			}
			return false;
		}

		static bool CanOpenForEdit(string[] paths, List<string> outNotEditablePaths, StatusQueryOptions statusQueryOptions)
		{
			bool hasLock = false;
			foreach (var item in paths)
			{
				if (!DB.CanEdit(item))
				{
					hasLock = true;
					outNotEditablePaths.Add(item);
				}
			}
			return !hasLock;
		}

		static bool IsOpenForEdit(string[] paths, List<string> outNotEditablePaths, StatusQueryOptions statusQueryOptions)
		{
			bool hasLock = false;
			foreach (var item in paths)
			{
				if (!DB.CanEdit(item))
				{
					hasLock = true;
					outNotEditablePaths.Add(item);
				}
			}
			return !hasLock;
		}

		static bool MakeEditable(string[] paths, string prompt, List<string> outNotEditablePaths)
		{
			bool hasLock = false;
			foreach (var item in paths)
			{
				if (!DB.CanEdit(item))
				{
					hasLock = true;
					outNotEditablePaths.Add(item);
				}
			}
			return !hasLock;
		}

		static string[] OnWillSaveAssets(string[] paths)
		{
			using var _ = ListPool<string>.Get(out var editablePaths);
			bool hasLock = false;
			foreach (var item in paths)
			{
				if (!AssetImporter.GetAtPath(item))
				{
					editablePaths.Add(item);
					continue;
				}
				if (!DB.CanEdit(item))
				{
					hasLock = true;
				}
				else
				{
					editablePaths.Add(item);
				}
			}
			if (hasLock)
			{
				return editablePaths.ToArray();
			}
			else
			{
				return paths;
			}
		}

	}
}