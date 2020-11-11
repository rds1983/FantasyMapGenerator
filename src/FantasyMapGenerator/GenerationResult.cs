using System;
using System.Collections.Generic;
using System.Drawing;
using GoRogue;
using GoRogue.MapViews;

namespace FantasyMapGenerator
{
	public class GenerationResult : IMapView<bool>
	{
		private static readonly HashSet<WorldMapTileType> _passableTypes = new HashSet<WorldMapTileType>
		{
			WorldMapTileType.Land, WorldMapTileType.Road, WorldMapTileType.Forest
		};

		private readonly WorldMapTileType[,] _data;

		public WorldMapTileType this[int x, int y]
		{
			get
			{
				return _data[x, y];
			}

			set
			{
				_data[x, y] = value;
			}
		}

		public int Width
		{
			get
			{
				return _data.GetLength(0);
			}
		}

		public int Height
		{
			get
			{
				return _data.GetLength(1);
			}
		}

		public List<LocationInfo> Locations { get; } = new List<LocationInfo>();

		public bool this[int index1D] => throw new NotImplementedException();

		public bool this[Coord pos] => IsPassable(pos.X, pos.Y);

		bool IMapView<bool>.this[int x, int y] => IsPassable(x, y);

		public GenerationResult(WorldMapTileType[,] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			_data = data;
		}

		public bool IsPassable(int x, int y)
		{
			return _passableTypes.Contains(this[x, y]) &&
				!IsNear(new Point(x, y), WorldMapTileType.Water, 3) &&
				!IsNear(new Point(x, y), WorldMapTileType.Mountain, 3) &&
				Utils.Random.Next(0, 10) != 0;
		}

		public WorldMapTileType GetWorldMapTileType(int x, int y, WorldMapTileType def = WorldMapTileType.Water)
		{
			if (x < 0 || x >= Width ||
				y < 0 || y >= Height)
			{
				return def;
			}

			return this[x, y];
		}

		public WorldMapTileType GetWorldMapTileType(Point p, WorldMapTileType def = WorldMapTileType.Water)
		{
			return GetWorldMapTileType(p.X, p.Y, def);
		}

		public void SetWorldMapTileType(int x, int y, WorldMapTileType type)
		{
			this[x, y] = type;
		}

		public void SetWorldMapTileType(Point p, WorldMapTileType type)
		{
			SetWorldMapTileType(p.X, p.Y, type);
		}

		public bool IsWater(int x, int y)
		{
			return GetWorldMapTileType(x, y) == WorldMapTileType.Water;
		}

		public bool IsWater(Point p)
		{
			return IsWater(p.X, p.Y);
		}

		public bool IsMountain(int x, int y)
		{
			return GetWorldMapTileType(x, y) == WorldMapTileType.Mountain;
		}

		public bool IsForest(int x, int y)
		{
			return GetWorldMapTileType(x, y) == WorldMapTileType.Forest;
		}

		public bool IsRoad(int x, int y)
		{
			return GetWorldMapTileType(x, y) == WorldMapTileType.Road;
		}

		public bool IsLand(int x, int y)
		{
			return GetWorldMapTileType(x, y) == WorldMapTileType.Land;
		}

		public bool IsLand(Point p)
		{
			return IsLand(p.X, p.Y);
		}

		public bool IsNear(Point p, WorldMapTileType tileType, int radius = 1)
		{
			for(var y = p.Y - radius; y <= p.Y + radius; ++y)
			{
				for(var x = p.X - radius; x <= p.X + radius; ++x)
				{
					if (x < 0 || x >= Width || y < 0 || y >= Height)
					{
						continue;
					}

					if (x == p.X && y == p.Y)
					{
						continue;
					}

					if (this[x, y] == tileType)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}