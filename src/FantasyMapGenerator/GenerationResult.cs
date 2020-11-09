using System;
using System.Collections.Generic;
using System.Drawing;

namespace FantasyMapGenerator
{
	public class GenerationResult
	{
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

		public GenerationResult(WorldMapTileType[,] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			_data = data;
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

		public bool IsNear(Point p, WorldMapTileType tileType)
		{
			foreach (var d in Utils.AllDirections)
			{
				if (GetWorldMapTileType(p + d) == tileType)
				{
					return true;
				}
			}

			return false;
		}

		public bool IsRoadPlaceable(Point p)
		{
			var tileType = GetWorldMapTileType(p);
			return (tileType == WorldMapTileType.Land ||
					tileType == WorldMapTileType.Road ||
					tileType == WorldMapTileType.Forest) && 
					!IsNear(p, WorldMapTileType.Mountain);
		}
	}
}