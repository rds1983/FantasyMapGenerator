using System;
using System.Collections.Generic;
using System.Drawing;
using GoRogue;
using GoRogue.MapViews;

namespace FantasyMapGenerator
{
	public class GenerationResult : IMapView<bool>
	{
		private static readonly HashSet<TileType> _passableTypes = new HashSet<TileType>
		{
			TileType.Sand, TileType.Land, TileType.Forest, TileType.Road, TileType.River
		};

		private readonly Tile[,] _tiles;

		public Tile this[int x, int y] => _tiles[x, y];
		public Tile this[Point p] => this[p.X, p.Y];


		public int Width
		{
			get
			{
				return _tiles.GetLength(0);
			}
		}

		public int Height
		{
			get
			{
				return _tiles.GetLength(1);
			}
		}

		public float DeepWaterLevel { get; set; }
		public float ShallowWaterLevel { get; set; }
		public float SandLevel { get; set; }
		public float LandLevel { get; set; }
		public float RockLevel { get; set; }

		public readonly List<River> Rivers = new List<River>();
		public readonly List<RiverGroup> RiverGroups = new List<RiverGroup>();

		public List<LocationInfo> Locations { get; } = new List<LocationInfo>();

		public bool this[int index1D] => throw new NotImplementedException();

		public bool this[Coord pos] => IsPassable(pos.X, pos.Y);

		bool IMapView<bool>.this[int x, int y] => IsPassable(x, y);

		public GenerationResult(int width, int height)
		{
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}

			_tiles = new Tile[width, height];

			for (var x = 0; x < Width; ++x)
			{
				for (var y = 0; y < Height; ++y)
				{
					_tiles[x, y] = new Tile(this, x, y);
				}
			}
		}

		public TileType GetTileType(int x, int y, TileType def = TileType.ShallowWater)
		{
			if (x < 0 || x >= Width ||
				y < 0 || y >= Height)
			{
				return def;
			}

			return this[x, y].TileType;
		}

		public TileType GetTileType(Point p, TileType def = TileType.ShallowWater) => GetTileType(p.X, p.Y, def);

		public bool IsPassable(int x, int y)
		{
			return _passableTypes.Contains(this[x, y].TileType) &&
				!IsNear(new Point(x, y), TileType.ShallowWater, 3) &&
				!IsNear(new Point(x, y), TileType.Rock, 3) &&
				MathHelper.Random.Next(0, 10) != 0;
		}

		public bool IsNear(Point p, TileType tileType, int radius = 1)
		{
			for (var y = p.Y - radius; y <= p.Y + radius; ++y)
			{
				for (var x = p.X - radius; x <= p.X + radius; ++x)
				{
					if (x < 0 || x >= Width || y < 0 || y >= Height)
					{
						continue;
					}

					if (x == p.X && y == p.Y)
					{
						continue;
					}

					if (this[x, y].TileType == tileType)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void Clear()
		{
			for (var x = 0; x < Width; ++x)
			{
				for (var y = 0; y < Height; ++y)
				{
					_tiles[x, y].Height = 0;
					_tiles[x, y].TileType = TileType.DeepWater;
				}
			}
		}

		public void UpdateTileTypes()
		{
			for (var x = 0; x < Width; ++x)
			{
				for (var y = 0; y < Height; ++y)
				{
					var tile = this[x, y];
					var heightValue = tile.Height;

					TileType tileType;
					if (heightValue < DeepWaterLevel)
					{
						tileType = TileType.DeepWater;
					}
					else if (heightValue < ShallowWaterLevel)
					{
						tileType = TileType.ShallowWater;
					}
					else if (heightValue < SandLevel)
					{
						tileType = TileType.Sand;
					}
					else if (heightValue < LandLevel)
					{
						tileType = TileType.Land;
					}
					else if (heightValue < RockLevel)
					{
						tileType = TileType.Rock;
					}
					else
					{
						tileType = TileType.Snow;
					}

					tile.TileType = tileType;
				}
			}
		}
	}
}