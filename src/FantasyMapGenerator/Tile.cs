using System.Collections.Generic;

namespace FantasyMapGenerator
{
	public class Tile
	{
		public GenerationResult Result { get; }
		public int X { get; }
		public int Y { get; }


		public float Height { get; set; }
		public TileType TileType { get; set; }

		public bool Collidable => TileType != TileType.DeepWater && TileType != TileType.ShallowWater && TileType != TileType.River;

		internal List<River> Rivers = new List<River>();

		internal int RiverSize { get; set; }

		public Tile Top => Result[X, MathHelper.Mod(Y - 1, Result.Height)];
		public Tile Bottom => Result[X, MathHelper.Mod(Y + 1, Result.Height)];
		public Tile Left => Result[MathHelper.Mod(X - 1, Result.Width), Y];
		public Tile Right => Result[MathHelper.Mod(X + 1, Result.Width), Y];

		public Tile(GenerationResult result, int x, int y)
		{
			Result = result;
			X = x;
			Y = y;
		}

		public void SetRiverPath(River river)
		{
			if (!Collidable)
				return;

			if (!Rivers.Contains(river))
			{
				Rivers.Add(river);
			}
		}

		private void SetRiverTile(River river)
		{
			SetRiverPath(river);
			TileType = TileType.River;
			Height = 0;
		}

		public void DigRiver(River river, int size)
		{
			SetRiverTile(river);
			RiverSize = size;

			if (size == 1)
			{
				if (Bottom != null)
				{
					Bottom.SetRiverTile(river);
					if (Bottom.Right != null) Bottom.Right.SetRiverTile(river);
				}
				if (Right != null) Right.SetRiverTile(river);
			}

			if (size == 2)
			{
				if (Bottom != null)
				{
					Bottom.SetRiverTile(river);
					if (Bottom.Right != null) Bottom.Right.SetRiverTile(river);
				}
				if (Right != null)
				{
					Right.SetRiverTile(river);
				}
				if (Top != null)
				{
					Top.SetRiverTile(river);
					if (Top.Left != null) Top.Left.SetRiverTile(river);
					if (Top.Right != null) Top.Right.SetRiverTile(river);
				}
				if (Left != null)
				{
					Left.SetRiverTile(river);
					if (Left.Bottom != null) Left.Bottom.SetRiverTile(river);
				}
			}

			if (size == 3)
			{
				if (Bottom != null)
				{
					Bottom.SetRiverTile(river);
					if (Bottom.Right != null) Bottom.Right.SetRiverTile(river);
					if (Bottom.Bottom != null)
					{
						Bottom.Bottom.SetRiverTile(river);
						if (Bottom.Bottom.Right != null) Bottom.Bottom.Right.SetRiverTile(river);
					}
				}
				if (Right != null)
				{
					Right.SetRiverTile(river);
					if (Right.Right != null)
					{
						Right.Right.SetRiverTile(river);
						if (Right.Right.Bottom != null) Right.Right.Bottom.SetRiverTile(river);
					}
				}
				if (Top != null)
				{
					Top.SetRiverTile(river);
					if (Top.Left != null) Top.Left.SetRiverTile(river);
					if (Top.Right != null) Top.Right.SetRiverTile(river);
				}
				if (Left != null)
				{
					Left.SetRiverTile(river);
					if (Left.Bottom != null) Left.Bottom.SetRiverTile(river);
				}
			}

			if (size == 4)
			{

				if (Bottom != null)
				{
					Bottom.SetRiverTile(river);
					if (Bottom.Right != null) Bottom.Right.SetRiverTile(river);
					if (Bottom.Bottom != null)
					{
						Bottom.Bottom.SetRiverTile(river);
						if (Bottom.Bottom.Right != null) Bottom.Bottom.Right.SetRiverTile(river);
					}
				}
				if (Right != null)
				{
					Right.SetRiverTile(river);
					if (Right.Right != null)
					{
						Right.Right.SetRiverTile(river);
						if (Right.Right.Bottom != null) Right.Right.Bottom.SetRiverTile(river);
					}
				}
				if (Top != null)
				{
					Top.SetRiverTile(river);
					if (Top.Right != null)
					{
						Top.Right.SetRiverTile(river);
						if (Top.Right.Right != null) Top.Right.Right.SetRiverTile(river);
					}
					if (Top.Top != null)
					{
						Top.Top.SetRiverTile(river);
						if (Top.Top.Right != null) Top.Top.Right.SetRiverTile(river);
					}
				}
				if (Left != null)
				{
					Left.SetRiverTile(river);
					if (Left.Bottom != null)
					{
						Left.Bottom.SetRiverTile(river);
						if (Left.Bottom.Bottom != null) Left.Bottom.Bottom.SetRiverTile(river);
					}

					if (Left.Left != null)
					{
						Left.Left.SetRiverTile(river);
						if (Left.Left.Bottom != null) Left.Left.Bottom.SetRiverTile(river);
						if (Left.Left.Top != null) Left.Left.Top.SetRiverTile(river);
					}

					if (Left.Top != null)
					{
						Left.Top.SetRiverTile(river);
						if (Left.Top.Top != null) Left.Top.Top.SetRiverTile(river);
					}
				}
			}
		}

		public int GetRiverNeighborCount(River river)
		{
			int count = 0;
			if (Left != null && Left.Rivers.Count > 0 && Left.Rivers.Contains(river))
				count++;
			if (Right != null && Right.Rivers.Count > 0 && Right.Rivers.Contains(river))
				count++;
			if (Top != null && Top.Rivers.Count > 0 && Top.Rivers.Contains(river))
				count++;
			if (Bottom != null && Bottom.Rivers.Count > 0 && Bottom.Rivers.Contains(river))
				count++;
			return count;
		}

		public RiverDirection GetLowestNeighbor()
		{
			float left = Left.Height;
			float right = Right.Height;
			float bottom = Bottom.Height;
			float top = Top.Height;

			if (left < right && left < top && left < bottom)
				return RiverDirection.Left;
			else if (right < left && right < top && right < bottom)
				return RiverDirection.Right;
			else if (top < left && top < right && top < bottom)
				return RiverDirection.Top;
			else if (bottom < top && bottom < right && bottom < left)
				return RiverDirection.Bottom;
			else
				return RiverDirection.Bottom;
		}
	}
}
