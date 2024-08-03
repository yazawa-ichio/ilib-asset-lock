using ILib.GoogleApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ILib.AssetLock
{
	public class GoogleSpreadSheetDB : ILockDB
	{
		class CategoryData
		{
			public string Category;

			public Dictionary<string, LockData> Data = new Dictionary<string, LockData>();

		}

		[SerializeField]
		string m_SheetId = "";


		Dictionary<string, CategoryData> m_Dic = new Dictionary<string, CategoryData>();

		LockData Get(AssetLockTarget target, string path)
		{
			if (m_Client == null)
			{
				_ = Load();
			}
			if (!m_Dic.TryGetValue(target.Category, out var data))
			{
				data = m_Dic[target.Category] = new CategoryData();
			}
			if (!data.Data.TryGetValue(path, out var lockData))
			{
				lockData = data.Data[path] = new LockData()
				{
					Path = path,
				};
			}
			lockData.TargetData = target;
			return lockData;
		}


		SpreadSheetClient m_Client;
		SpreadSheet m_Sheet;

		async Task Load()
		{
			m_Client = new SpreadSheetClient();
			m_Sheet = await m_Client.Get(m_SheetId);
			foreach (var sheet in m_Sheet.Sheets)
			{
				var tmp = await m_Client.GetValues(m_SheetId, $"'{sheet.Properties.Title}'");
				if (tmp.Values == null) continue;
				if (!m_Dic.TryGetValue(sheet.Properties.Title, out var data))
				{
					data = m_Dic[sheet.Properties.Title] = new CategoryData()
					{
						Category = sheet.Properties.Title,
					};
				}
				for (int i = 0; i < tmp.Values.Count; i++)
				{
					List<string> row = tmp.Values[i];
					string user = row.Count > 0 ? row[0] : "";
					string status = row.Count > 1 ? row[1] : "";
					string path = row.Count > 2 ? row[2] : "";
					string gitHash = row.Count > 3 ? row[3] : "";
					if (string.IsNullOrEmpty(path))
					{
						continue;
					}
					if (!data.Data.TryGetValue(path, out var lockData))
					{
						lockData = data.Data[path] = new LockData()
						{
							Path = path,
						};
					}
					lockData.User = user;
					Enum.TryParse<LockStatus>(status, out lockData.Status);
					lockData.GitHash = gitHash;
					lockData.Id = i + 1;
				}
			}
			Repaint();
		}

		public LockData GetData(AssetLockTarget target, string path)
		{
			return Get(target, path);
		}

		public async void TryLock(AssetLockTarget target, string path, Action onLock)
		{
			try
			{
				await Load();
				var data = Get(target, path);
				if (data.Status == LockStatus.Lock)
				{
					return;
				}
				if (!m_Sheet.Sheets.Any(x => x.Properties.Title == target.Category))
				{
					await m_Client.AddSheet(m_SheetId, target.Category);
					await m_Client.Append(m_SheetId, new ValueRange()
					{
						Range = $"'{target.Category}'!A1",
						Values = new List<List<string>>()
						{
							new List<string>(){ "User","Status", "Path", "GitHash" }
						}
					});
				}
				int id = data.Id;
				var values = new List<string>() { Git.User(), LockStatus.Lock.ToString(), path, Git.GetCurrentHash() };
				if (id == 0)
				{
					await m_Client.Append(m_SheetId, new ValueRange()
					{
						Range = $"'{target.Category}'!A1:D1",
						Values = new List<List<string>>() { values },
					});
				}
				else
				{
					await m_Client.Update(m_SheetId, new ValueRange()
					{
						Range = $"'{target.Category}'!A{id}:D{id}",
						Values = new List<List<string>>() { values },
					});
				}
				await Load();
				onLock?.Invoke();
				Repaint();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}
		}

		public async void Unlock(AssetLockTarget target, string path)
		{
			try
			{
				await Load();
				var data = Get(target, path);
				if (!data.IsMyLock())
				{
					return;
				}
				var values = new List<string>() { "", LockStatus.Unlock.ToString(), path, Git.GetCurrentHash() };
				await m_Client.Update(m_SheetId, new ValueRange()
				{
					Range = $"'{target.Category}'!A{data.Id}:D{data.Id}",
					Values = new List<List<string>>() { values }
				});
				await Load();
				Repaint();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}
		}

		void Repaint()
		{
			var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			var inspector = EditorWindow.GetWindow(type);
			inspector.Repaint();
		}

		public void OnGUI()
		{
			m_SheetId = EditorGUILayout.TextField("SheetId", m_SheetId);
		}
	}
}