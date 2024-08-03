using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetLock
{

	public class AssetLockSettingsProvider : SettingsProvider
	{
		readonly static Type[] s_LockDBTypes;
		readonly static string[] s_LockDBNames;

		static AssetLockSettingsProvider()
		{
			s_LockDBTypes = Enumerable.Empty<Type>()
				.Append(default(Type))
				.Concat(TypeCache.GetTypesDerivedFrom<ILockDB>())
				.ToArray();
			s_LockDBNames = s_LockDBTypes.Select(x => x?.Name ?? "NONE").ToArray();
		}

		public static string ProjectSettingsPath => "ProjectSettings/ILib.AssetLock.";

		[SettingsProvider]
		public static SettingsProvider CreateProvider()
		{
			var provider = new AssetLockSettingsProvider("Project/ILib AssetLock", SettingsScope.Project);
			return provider;
		}

		public AssetLockSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		public override void OnGUI(string searchContext)
		{
			using var change = new EditorGUI.ChangeCheckScope();

			var settings = AssetLockSettings.I;
			{
				EditorGUILayout.LabelField("[LockDB]");
				var current = settings.LockDB;
				var index = Array.IndexOf(s_LockDBTypes, current?.GetType());
				var ret = EditorGUILayout.Popup(index, s_LockDBNames);
				if (index != ret)
				{
					if (ret <= 0)
					{
						settings.LockDB = current = null;
					}
					else
					{
						settings.LockDB = current = (ILockDB)Activator.CreateInstance(s_LockDBTypes[ret]);
					}
				}
				current?.OnGUI();
			}
			{
				EditorGUILayout.LabelField("[Lock Target]");
				AssetLockTarget delete = null;
				foreach (var target in settings.Targets)
				{
					using (new GUILayout.HorizontalScope())
					{
						target.Category = EditorGUILayout.TextField("Category", target.Category);
						target.Pattern = EditorGUILayout.TextField("Pattern", target.Pattern);
						if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
						{
							delete = target;
						}
					}
					target.Type = (LockType)EditorGUILayout.EnumPopup("Type", target.Type);
				}
				if (delete != null)
				{
					settings.Targets.Remove(delete);
				}
				if (GUILayout.Button("Add"))
				{
					settings.Targets.Add(new AssetLockTarget());
				}
			}
			if (change.changed)
			{
				settings.Save();
			}

		}
	}
}