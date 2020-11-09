using System;
using System.Reflection;

namespace FantasyMapGenerator
{
	public static class GenerationEnvironment
	{
		public static Action<string> InfoHandler = Console.WriteLine;

		public static string Version
		{
			get
			{
				var assembly = typeof(GenerationEnvironment).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		public static void LogInfo(string message, params object[] args)
		{
			if (InfoHandler == null)
			{
				return;
			}

			InfoHandler(Utils.FormatMessage(message, args));
		}
	}
}
