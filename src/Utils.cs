using System;
using System.Drawing;
using System.Reflection;

namespace FantasyMapGenerator
{
	internal static class Utils
	{
		public static Point Zero = new Point(0, 0);

		public static readonly Size[] AllDirections = new Size[]
		{
			new Size(0, -1),
			new Size(-1, 0),
			new Size(1, 0),
			new Size(0, 1),
			new Size(-1, -1),
			new Size(1, -1),
			new Size(-1, 1),
			new Size(1, 1),
		};

		public static readonly Size[] FourDirections = new Size[]
		{
			new Size(0, -1),
			new Size(-1, 0),
			new Size(-1, -1),
			new Size(1, -1),
		};

		public static string Version
		{
			get
			{
				var assembly = typeof(Utils).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		public static string FormatMessage(string message, params object[] args)
		{
			string str;
			try
			{
				if (args != null && args.Length > 0)
				{
					str = string.Format(message, args);
				}
				else
				{
					str = message;
				}
			}
			catch (FormatException)
			{
				str = message;
			}

			return str;
		}

		public static void Fill<T>(this T[] array, T value)
		{
			for (var i = 0; i < array.Length; ++i)
			{
				array[i] = value;
			}
		}

		public static void Fill<T>(this T[,] array, T value)
		{
			for (var i = 0; i < array.GetLength(0); ++i)
			{
				for (var j = 0; j < array.GetLength(1); ++j)
				{
					array[i, j] = value;
				}
			}
		}
	}
}