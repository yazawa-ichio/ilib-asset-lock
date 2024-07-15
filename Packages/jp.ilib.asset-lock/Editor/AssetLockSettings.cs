using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetLock
{
	[Serializable]
	public class AssetLockSettings
	{

		static readonly string s_Path = "ProjectSettings/ILib.AssetLockSettings.json";

		static AssetLockSettings s_Instance;

		public static AssetLockSettings I
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = new AssetLockSettings();
					if (File.Exists(s_Path))
					{
						var json = File.ReadAllText(s_Path);
						JsonUtility.FromJsonOverwrite(json, s_Instance);
					}
				}
				return s_Instance;
			}
		}

		[SerializeReference]
		public ILockDB LockDB;

		public List<AssetLockTarget> Targets = new();

		private AssetLockSettings() { }

		public void Save()
		{
			var json = EditorJsonUtility.ToJson(this, true);
			File.WriteAllText(s_Path, json);
		}

	}
}