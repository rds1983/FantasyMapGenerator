using System;
using System.Reflection;

namespace FantasyMapGenerator
{
	public static class GenerationEnvironment
	{
		public static string Version
		{
			get
			{
				var assembly = typeof(GenerationEnvironment).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}
	}
}
