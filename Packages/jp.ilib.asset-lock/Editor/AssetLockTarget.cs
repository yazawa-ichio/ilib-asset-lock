using System;
using System.Text.RegularExpressions;

namespace ILib.AssetLock
{
	[Serializable]
	public class AssetLockTarget
	{
		public string Category;

		public string Pattern;

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