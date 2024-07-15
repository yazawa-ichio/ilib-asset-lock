using System.Diagnostics;
namespace ILib.AssetLock
{
	public static class Git
	{
		static string s_User;
		static string s_Email;

		static Git()
		{
			s_User = Command("config user.name").Trim();
			s_Email = Command("config user.email").Trim();
		}

		public static string User()
		{
			return s_User;
		}

		public static string GetCurrentHash()
		{
			return Command("rev-parse HEAD").Trim();
		}

		public static bool MergeBaseCheck(string hash)
		{
			var prosess = new Process();
			prosess.StartInfo = new ProcessStartInfo()
			{
				FileName = "git",
				Arguments = $"merge-base --is-ancestor {hash}  HEAD",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
			};
			prosess.Start();
			prosess.WaitForExit();
			return prosess.ExitCode == 0;
		}

		static string Command(string arguments)
		{
			var prosess = new Process();
			prosess.StartInfo = new ProcessStartInfo()
			{
				FileName = "git",
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
			};
			prosess.Start();
			return prosess.StandardOutput.ReadToEnd().Trim();
		}
	}
}