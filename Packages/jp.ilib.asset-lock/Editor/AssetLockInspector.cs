using System.IO;
using UnityEditor;
using UnityEngine;

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
					if (lockData.TargetData.Type != LockType.AllowSave)
					{
						if (GUILayout.Button("Reload"))
						{
							EditorApplication.delayCall += () =>
							{
								if (!path.EndsWith(".unity"))
								{
									foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
									{
										if (obj is not GameObject && obj is not Component)
										{
											Resources.UnloadAsset(obj);
										}
									}
								}
								new FileInfo(path).LastWriteTime = new FileInfo(path).LastWriteTime.AddSeconds(1);
								AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
								Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
							};
						}
					}
					if (lockData.TargetData.Type != LockType.DontOpen)
					{
						lockData.EditMode = GUILayout.Toggle(lockData.EditMode, "EditMode");
					}
				}
			}
			finally
			{
				GUI.enabled = enabled;
			}
		}

	}
}