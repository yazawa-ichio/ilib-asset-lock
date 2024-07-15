using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Pool;

namespace ILib.AssetLock
{
	public static class AssetLockInspector
	{
		static ILockDB DB => AssetLockSettings.I.LockDB;

		[InitializeOnLoadMethod]
		static void Init()
		{
			Editor.finishedDefaultHeaderGUI -= OnHeaderGUI;
			Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
		}

		private static void OnHeaderGUI(Editor editor)
		{
			var path = AssetDatabase.GetAssetPath(editor.target);
			if (!DB.TryGetLockData(path, out var lockData))
			{
				return;
			}
			var enabled = GUI.enabled;
			GUI.enabled = true;
			try
			{
				if (lockData.IsMyLock())
				{
					EditorGUILayout.LabelField("MyLock");
					if (GUILayout.Button("Unlock"))
					{
						DB.Unlock(path);
					}
				}
				else
				{
					EditorGUILayout.LabelField("Lock");
					if (lockData.Status == LockStatus.Lock)
					{
						GUILayout.Label("User:" + lockData.User);
						if (GUILayout.Button("TryLock"))
						{
							DB.TryLock(path);
						}
					}
					else
					{
						if (GUILayout.Button("TryLock"))
						{
							DB.TryLock(path);
						}
					}
				}
			}
			finally
			{
				GUI.enabled = enabled;
			}
		}

	}

	public class AssetLockImportHook : AssetModificationProcessor
	{
		static ILockDB DB => AssetLockSettings.I.LockDB;

		[OnOpenAsset(0)]
		static bool OnOpen(int instanceID, int line)
		{

			return true;
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