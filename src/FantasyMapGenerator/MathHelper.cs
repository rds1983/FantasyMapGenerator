using System;
using System.Drawing;

namespace FantasyMapGenerator
{
	internal static class MathHelper
	{
		public static Point Zero = new Point(0, 0);

		public static readonly Random Random = new Random();

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

		public static float Distance(PointF a, PointF b)
		{
			var delta = new PointF(b.X - a.X, b.Y - a.Y);

			return (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
		}

		public static float Distance(Point a, Point b)
		{
			return Distance(a.ToPointF(), b.ToPointF());
		}

		public static PointF ToPointF(this Point a)
		{
			return new PointF(a.X, a.Y);
		}

		public static int Mod(int x, int m)
		{
			int r = x % m;
			return r < 0 ? r + m : r;
		}

		public static int RandomRange(int min, int max) => Random.Next(min, max);
	}
}