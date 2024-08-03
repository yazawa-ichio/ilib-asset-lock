using System;
using System.Text.RegularExpressions;

namespace ILib.AssetLock
{
	public enum LockType
	{
		DontSave,
		DontOpen,
		AllowSave,
	}

	[Serializable]
	public class AssetLockTarget
	{
		public string Category;

		public string Pattern;

		public LockType Type;

		[NonSerialized]
		string m_RegexPattern;
		[NonSerialized]
		Regex m_Regex;
		public Regex Regex
		{
			get
			{
				if (m_RegexPattern != Pattern)
				{
					m_RegexPattern = Pattern;
					m_Regex = new Regex(Pattern);
				}
				return m_Regex;
			}
		}
	}
}