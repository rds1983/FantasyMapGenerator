using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TinkerWorX.AccidentalNoiseLibrary;

namespace FantasyMapGenerator
{
	public class LandGenerator
	{
		private const int MinimumIslandSize = 1000;

		private readonly struct TileDirection
		{
			public readonly Tile Tile;
			public readonly RiverDirection Direction;

			public TileDirection(Tile tile, RiverDirection direction)
			{
				Tile = tile;
				Direction = direction;
			}
		}

		protected ImplicitFractal HeightMap;
		private bool[,] _islandMask;
		private GenerationConfig _config;
		private GenerationResult _result;

		public int Size
		{
			get
			{
				return _config.WorldSize;
			}
		}

		public GenerationResult Result => _result;

		public LandGenerator(GenerationConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			_config = config;

			HeightMap = new ImplicitFractal(FractalType.Multi,
								 BasisType.Simplex,
								 InterpolationType.Quintic)
			{
				Octaves = config.TerrainOctaves,
				Frequency = config.TerrainFrequency,
				Seed = MathHelper.Random.Next(0, int.MaxValue)
			};

			_result = new GenerationResult(Size, Size);
		}

		private void LogInfo(string msg) => _config.LogInfo(msg);

		private void ReportNextStep(string name)
		{
			LogInfo(name);

			if (_config.NextStepCallback == null)
			{
				return;
			}

			_config.NextStepCallback(name);
		}

		private void ProcessColumn(int x)
		{
			for (var y = 0; y < Size; y++)
			{
				// WRAP ON BOTH AXIS
				// Noise range
				float x1 = 0, x2 = 2;
				float y1 = 0, y2 = 2;
				float dx = x2 - x1;
				float dy = y2 - y1;

				// Sample noise at smaller intervals
				float s = x / (float)Size;
				float t = y / (float)Size;

				// Calculate our 4D coordinates
				float nx = x1 + MathF.Cos(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
				float ny = y1 + MathF.Cos(t * 2 * MathF.PI) * dy / (2 * MathF.PI);
				float nz = x1 + MathF.Sin(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
				float nw = y1 + MathF.Sin(t * 2 * MathF.PI) * dy / (2 * MathF.PI);

				float heightValue = (float)HeightMap.Get(nx, ny, nz, nw);

				var tile = _result[x, y];
				tile.Height = heightValue;
			}
		}

		public void GenerateHeightMap()
		{
			Parallel.For(0, Size, x => ProcessColumn(x));

			// Normalize tile heights
			LogInfo("Normalizing tile heights");
			var minHeight = float.MaxValue;
			var maxHeight = float.MinValue;
			for (var x = 0; x < Size; ++x)
			{
				for (var y = 0; y < Size; ++y)
				{
					var height = _result[x, y].Height;

					if (height < minHeight)
					{
						minHeight = height;
					}

					if (height > maxHeight)
					{
						maxHeight = height;
					}
				}
			}

			for (var x = 0; x < Size; ++x)
			{
				for (var y = 0; y < Size; ++y)
				{
					var tile = _result[x, y];
					var heightValue = (tile.Height - minHeight) / (maxHeight - minHeight);
					tile.Height = heightValue;
				}
			}
		}

		private float CalculateMinimum(float minimum, float part)
		{
			var maximum = minimum + 0.01f;
			while (maximum < 1.0f)
			{
				int c = 0;
				Parallel.For(0, Size, y =>
				{
					for (int x = 0; x < Size; ++x)
					{
						var h = _result[x, y].Height;
						if (minimum <= h && h <= maximum)
						{
							Interlocked.Increment(ref c);
						}
					}
				});

				float prop = (float)c / (Size * Size);
				if (prop >= part)
				{
					break;
				}

				maximum += 0.01f;
			}

			return maximum;
		}

		private void CalculateMinimums()
		{
			LogInfo("Calculating minimums");
			_result.DeepWaterLevel = CalculateMinimum(0.0f, _config.DeepWaterPart);
			_result.ShallowWaterLevel = CalculateMinimum(_result.DeepWaterLevel, _config.ShallowWaterPart);
			_result.SandLevel = CalculateMinimum(_result.ShallowWaterLevel, _config.SandPart);
			_result.LandLevel = CalculateMinimum(_result.SandLevel, _config.LandPart);
			_result.RockLevel = CalculateMinimum(_result.LandLevel, _config.RockPart);
		}

		private void ClearMask()
		{
			_islandMask.Fill(false);
		}

		private List<Point> Build(int x, int y, Func<Point, bool> addCondition)
		{
			// Clear mask
			List<Point> result = new List<Point>();

			Stack<Point> toProcess = new Stack<Point>();

			toProcess.Push(new Point(x, y));

			while (toProcess.Count > 0)
			{
				Point top = toProcess.Pop();

				if (top.X < 0 ||
						top.X >= Size ||
						top.Y < 0 ||
						top.Y >= Size ||
						_islandMask[top.Y, top.X] ||
						!addCondition(top))
				{
					continue;
				}

				result.Add(top);
				_islandMask[top.Y, top.X] = true;

				// Add adjancement tiles
				toProcess.Push(new Point(top.X - 1, top.Y));
				toProcess.Push(new Point(top.X, top.Y - 1));
				toProcess.Push(new Point(top.X + 1, top.Y));
				toProcess.Push(new Point(top.X, top.Y + 1));
			}

			return result;
		}

		private void RemoveTiles(string name, TileType[] tileTypes)
		{
			ReportNextStep($"Removing {name}...");

			var tilesReplaced = 0;

			ClearMask();

			// Next run remove small islands
			for (int y = 0; y < Size; ++y)
			{
				for (int x = 0; x < Size; ++x)
				{
					if (!_islandMask[y, x] && tileTypes.Contains(_result.GetTileType(x, y)))
					{
						var newValue = _result[x, y].Left.TileType;
						List<Point> island = Build(x, y, p => tileTypes.Contains(_result.GetTileType(p.X, p.Y)));

						if (island.Count < MinimumIslandSize)
						{
							// Remove small island
							foreach (var p in island)
							{
								_result[p].TileType = newValue;
								++tilesReplaced;
							}
						}
					}
				}
			}

			LogInfo($"Tiles replaced: {tilesReplaced}");
		}

		private void RemoveTiles(string name, TileType tileType) => RemoveTiles(name, new TileType[] { tileType });

		private void RemoveNoise(string name, TileType tileType)
		{
			ReportNextStep($"Removing {name}...");

			var iterations = 0;
			var tilesReplaced = 0;
			while (true)
			{
				++iterations;
				var changed = false;
				for (int y = 0; y < Size; ++y)
				{
					for (int x = 0; x < Size; ++x)
					{
						if (_result.GetTileType(x, y) == tileType)
						{
							continue;
						}

						for (var i = 0; i < MathHelper.FourDirections.Length; ++i)
						{
							var d = MathHelper.FourDirections[i];
							var p = new Point(x + d.Width, y + d.Height);
							var p2 = new Point(x - d.Width, y - d.Height);

							if (_result.GetTileType(p, tileType) == tileType &&
								_result.GetTileType(p2, tileType) == tileType)
							{
								// Turn into new value
								_result[x, y].TileType = tileType;
								changed = true;
								++tilesReplaced;
								break;
							}
						}
					}
				}

				if (!changed)
				{
					break;
				}
			}

			LogInfo($"Removal iterations: {iterations}");
			LogInfo($"Tiles replaced: {tilesReplaced}");

		}

		private void GenerateForests()
		{
			ReportNextStep("Generating forests...");

			var c = 0;

			var tilesCount = Size * Size;
			while (((float)c / tilesCount) < _config.ForestPart)
			{
				var locations = new Queue<Point>();
				for (var i = 0; i < 10; ++i)
				{
					// Find starting spot
					var tries = 100;
					var p = MathHelper.Zero;
					while (tries > 0)
					{
						--tries;
						p.X = MathHelper.RandomRange(0, _result.Width);
						p.Y = MathHelper.RandomRange(0, _result.Height);

						if (_result[p].TileType == TileType.Land)
						{
							locations.Enqueue(p);
							break;
						}
					}
				}

				// Grow forest from this spot
				while (locations.Count > 0 && ((float)c / tilesCount) < _config.ForestPart)
				{
					var p = locations.Dequeue();

					if (_result[p].TileType != TileType.Land ||
						_result.IsNear(p, TileType.ShallowWater) ||
						_result.IsNear(p, TileType.Rock))
					{
						continue;
					}

					_result[p].TileType = TileType.Forest;
					++c;

					for (var i = 0; i < 4; ++i)
					{
						var dist = MathHelper.RandomRange(0, 25);
						var angle = MathHelper.RandomRange(0, 360);

						var radAngle = Math.PI * angle / 180;

						var d = new Point(p.X + (int)(Math.Cos(radAngle) * dist), p.Y + (int)(Math.Sin(radAngle) * dist));

						if (d.X < 0 || d.Y < 0 || d.X >= _result.Width || d.Y >= _result.Height)
						{
							continue;
						}

						if (_result[d].TileType == TileType.Land)
						{
							locations.Enqueue(d);
						}
					}
				}
			}
		}

		private void FindPathToWater(TileDirection? td, ref River river)
		{
			while (td != null)
			{
				var tile = td.Value.Tile;
				var direction = td.Value.Direction;

				td = null;
				if (tile.Rivers.Contains(river))
					continue;

				// check if there is already a river on this tile
				if (tile.Rivers.Count > 0)
					river.Intersections++;

				river.AddTile(tile);

				// get neighbors
				Tile left = tile.Left;
				Tile right = tile.Right;
				Tile top = tile.Top;
				Tile bottom = tile.Bottom;

				float leftValue = int.MaxValue;
				float rightValue = int.MaxValue;
				float topValue = int.MaxValue;
				float bottomValue = int.MaxValue;

				// query height values of neighbors
				if (left != null && left.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(left))
					leftValue = left.Height;
				if (right != null && right.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(right))
					rightValue = right.Height;
				if (top != null && top.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(top))
					topValue = top.Height;
				if (bottom != null && bottom.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(bottom))
					bottomValue = bottom.Height;

				// if neighbor is existing river that is not this one, flow into it
				if (bottom != null && bottom.Rivers.Count == 0 && !bottom.Collidable)
					bottomValue = 0;
				if (top != null && top.Rivers.Count == 0 && !top.Collidable)
					topValue = 0;
				if (left != null && left.Rivers.Count == 0 && !left.Collidable)
					leftValue = 0;
				if (right != null && right.Rivers.Count == 0 && !right.Collidable)
					rightValue = 0;

				// override flow direction if a tile is significantly lower
				if (direction == RiverDirection.Left)
					if (MathF.Abs(rightValue - leftValue) < 0.1f)
						rightValue = int.MaxValue;
				if (direction == RiverDirection.Right)
					if (MathF.Abs(rightValue - leftValue) < 0.1f)
						leftValue = int.MaxValue;
				if (direction == RiverDirection.Top)
					if (MathF.Abs(topValue - bottomValue) < 0.1f)
						bottomValue = int.MaxValue;
				if (direction == RiverDirection.Bottom)
					if (MathF.Abs(topValue - bottomValue) < 0.1f)
						topValue = int.MaxValue;

				// find mininum
				float min = MathF.Min(MathF.Min(MathF.Min(leftValue, rightValue), topValue), bottomValue);

				// if no minimum found - exit
				if (min == int.MaxValue)
					continue;

				//Move to next neighbor
				if (min == leftValue)
				{
					if (left != null && left.Collidable)
					{
						if (river.CurrentDirection != RiverDirection.Left)
						{
							river.TurnCount++;
							river.CurrentDirection = RiverDirection.Left;
						}

						td = new TileDirection(left, direction);
					}
				}
				else if (min == rightValue)
				{
					if (right != null && right.Collidable)
					{
						if (river.CurrentDirection != RiverDirection.Right)
						{
							river.TurnCount++;
							river.CurrentDirection = RiverDirection.Right;
						}
						td = new TileDirection(right, direction);
					}
				}
				else if (min == bottomValue)
				{
					if (bottom != null && bottom.Collidable)
					{
						if (river.CurrentDirection != RiverDirection.Bottom)
						{
							river.TurnCount++;
							river.CurrentDirection = RiverDirection.Bottom;
						}
						td = new TileDirection(bottom, direction);
					}
				}
				else if (min == topValue)
				{
					if (top != null && top.Collidable)
					{
						if (river.CurrentDirection != RiverDirection.Top)
						{
							river.TurnCount++;
							river.CurrentDirection = RiverDirection.Top;
						}
						td = new TileDirection(top, direction);
					}
				}
			}
		}

		private void GenerateRivers()
		{
			ReportNextStep("Generate rivers..");
			int attempts = 0;
			int rivercount = _config.RiverCount;

			// Generate some rivers
			while (rivercount > 0 && attempts < _config.MaxRiverAttempts)
			{

				// Get a random tile
				int x = MathHelper.RandomRange(0, Size);
				int y = MathHelper.RandomRange(0, Size);
				Tile tile = _result[x, y];

				// validate the tile
				if (!tile.Collidable) continue;
				if (tile.Rivers.Count > 0) continue;

				if (tile.Height > _config.MinRiverHeight)
				{
					// Tile is good to start river from
					River river = new River(rivercount);

					// Figure out the direction this river will try to flow
					river.CurrentDirection = tile.GetLowestNeighbor();

					// Find a path to water
					FindPathToWater(new TileDirection(tile, river.CurrentDirection), ref river);

					// Validate the generated river 
					if (river.TurnCount < _config.MinRiverTurns || river.Tiles.Count < _config.MinRiverLength || river.Intersections > _config.MaxRiverIntersections)
					{
						//Validation failed - remove this river
						for (int i = 0; i < river.Tiles.Count; i++)
						{
							Tile t = river.Tiles[i];
							t.Rivers.Remove(river);
						}
					}
					else if (river.Tiles.Count >= _config.MinRiverLength)
					{
						//Validation passed - Add river to list
						_result.Rivers.Add(river);
						tile.Rivers.Add(river);
						rivercount--;
					}
				}
				attempts++;
			}
		}

		private void BuildRiverGroups()
		{
			LogInfo("Build river groups..");

			//loop each tile, checking if it belongs to multiple rivers
			for (var x = 0; x < Size; x++)
			{
				for (var y = 0; y < Size; y++)
				{
					Tile t = _result[x, y];

					if (t.Rivers.Count > 1)
					{
						// multiple rivers == intersection
						RiverGroup group = null;

						// Does a rivergroup already exist for this group?
						for (int n = 0; n < t.Rivers.Count; n++)
						{
							River tileriver = t.Rivers[n];
							for (int i = 0; i < _result.RiverGroups.Count; i++)
							{
								for (int j = 0; j < _result.RiverGroups[i].Rivers.Count; j++)
								{
									River river = _result.RiverGroups[i].Rivers[j];
									if (river.ID == tileriver.ID)
									{
										group = _result.RiverGroups[i];
									}
									if (group != null) break;
								}
								if (group != null) break;
							}
							if (group != null) break;
						}

						// existing group found -- add to it
						if (group != null)
						{
							for (int n = 0; n < t.Rivers.Count; n++)
							{
								if (!group.Rivers.Contains(t.Rivers[n]))
									group.Rivers.Add(t.Rivers[n]);
							}
						}
						else   //No existing group found - create a new one
						{
							group = new RiverGroup();
							for (int n = 0; n < t.Rivers.Count; n++)
							{
								group.Rivers.Add(t.Rivers[n]);
							}
							_result.RiverGroups.Add(group);
						}
					}
				}
			}
		}

		private void DigRiver(River river)
		{
			int counter = 0;

			// How wide are we digging this river?
			int size = MathHelper.RandomRange(1, 5);
			river.Length = river.Tiles.Count;

			// randomize size change
			int two = river.Length / 2;
			int three = two / 2;
			int four = three / 2;
			int five = four / 2;

			int twomin = two / 3;
			int threemin = three / 3;
			int fourmin = four / 3;
			int fivemin = five / 3;

			// randomize lenght of each size
			int count1 = MathHelper.RandomRange(fivemin, five);
			if (size < 4)
			{
				count1 = 0;
			}
			int count2 = count1 + MathHelper.RandomRange(fourmin, four);
			if (size < 3)
			{
				count2 = 0;
				count1 = 0;
			}
			int count3 = count2 + MathHelper.RandomRange(threemin, three);
			if (size < 2)
			{
				count3 = 0;
				count2 = 0;
				count1 = 0;
			}
			int count4 = count3 + MathHelper.RandomRange(twomin, two);

			// Make sure we are not digging past the river path
			if (count4 > river.Length)
			{
				int extra = count4 - river.Length;
				while (extra > 0)
				{
					if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
					else if (count2 > 0) { count2--; count3--; count4--; extra--; }
					else if (count3 > 0) { count3--; count4--; extra--; }
					else if (count4 > 0) { count4--; extra--; }
				}
			}

			// Dig it out
			for (int i = river.Tiles.Count - 1; i >= 0; i--)
			{
				Tile t = river.Tiles[i];

				if (counter < count1)
				{
					t.DigRiver(river, 4);
				}
				else if (counter < count2)
				{
					t.DigRiver(river, 3);
				}
				else if (counter < count3)
				{
					t.DigRiver(river, 2);
				}
				else if (counter < count4)
				{
					t.DigRiver(river, 1);
				}
				else
				{
					t.DigRiver(river, 0);
				}
				counter++;
			}
		}

		// Dig river based on a parent river vein
		private void DigRiver(River river, River parent)
		{
			int intersectionID = 0;
			int intersectionSize = 0;

			// determine point of intersection
			for (int i = 0; i < river.Tiles.Count; i++)
			{
				Tile t1 = river.Tiles[i];
				for (int j = 0; j < parent.Tiles.Count; j++)
				{
					Tile t2 = parent.Tiles[j];
					if (t1 == t2)
					{
						intersectionID = i;
						intersectionSize = t2.RiverSize;
					}
				}
			}

			int counter = 0;
			int intersectionCount = river.Tiles.Count - intersectionID;
			int size = MathHelper.RandomRange(intersectionSize, 5);
			river.Length = river.Tiles.Count;

			// randomize size change
			int two = river.Length / 2;
			int three = two / 2;
			int four = three / 2;
			int five = four / 2;

			int twomin = two / 3;
			int threemin = three / 3;
			int fourmin = four / 3;
			int fivemin = five / 3;

			// randomize length of each size
			int count1 = MathHelper.RandomRange(fivemin, five);
			if (size < 4)
			{
				count1 = 0;
			}
			int count2 = count1 + MathHelper.RandomRange(fourmin, four);
			if (size < 3)
			{
				count2 = 0;
				count1 = 0;
			}
			int count3 = count2 + MathHelper.RandomRange(threemin, three);
			if (size < 2)
			{
				count3 = 0;
				count2 = 0;
				count1 = 0;
			}
			int count4 = count3 + MathHelper.RandomRange(twomin, two);

			// Make sure we are not digging past the river path
			if (count4 > river.Length)
			{
				int extra = count4 - river.Length;
				while (extra > 0)
				{
					if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
					else if (count2 > 0) { count2--; count3--; count4--; extra--; }
					else if (count3 > 0) { count3--; count4--; extra--; }
					else if (count4 > 0) { count4--; extra--; }
				}
			}

			// adjust size of river at intersection point
			if (intersectionSize == 1)
			{
				count4 = intersectionCount;
				count1 = 0;
				count2 = 0;
				count3 = 0;
			}
			else if (intersectionSize == 2)
			{
				count3 = intersectionCount;
				count1 = 0;
				count2 = 0;
			}
			else if (intersectionSize == 3)
			{
				count2 = intersectionCount;
				count1 = 0;
			}
			else if (intersectionSize == 4)
			{
				count1 = intersectionCount;
			}
			else
			{
				count1 = 0;
				count2 = 0;
				count3 = 0;
				count4 = 0;
			}

			// dig out the river
			for (int i = river.Tiles.Count - 1; i >= 0; i--)
			{

				Tile t = river.Tiles[i];

				if (counter < count1)
				{
					t.DigRiver(river, 4);
				}
				else if (counter < count2)
				{
					t.DigRiver(river, 3);
				}
				else if (counter < count3)
				{
					t.DigRiver(river, 2);
				}
				else if (counter < count4)
				{
					t.DigRiver(river, 1);
				}
				else
				{
					t.DigRiver(river, 0);
				}
				counter++;
			}
		}

		private void DigRiverGroups()
		{
			LogInfo("Dig river groups");

			for (int i = 0; i < _result.RiverGroups.Count; i++)
			{
				RiverGroup group = _result.RiverGroups[i];
				River longest = null;

				//Find longest river in this group
				for (int j = 0; j < group.Rivers.Count; j++)
				{
					River river = group.Rivers[j];
					if (longest == null)
						longest = river;
					else if (longest.Tiles.Count < river.Tiles.Count)
						longest = river;
				}

				if (longest != null)
				{
					//Dig out longest path first
					DigRiver(longest);

					for (int j = 0; j < group.Rivers.Count; j++)
					{
						River river = group.Rivers[j];
						if (river != longest)
						{
							DigRiver(river, longest);
						}
					}
				}
			}
		}

		public void Generate()
		{
			_islandMask = new bool[Size, Size];
			_result.Clear();

			ReportNextStep("Generating height map...");
			GenerateHeightMap();
			CalculateMinimums();
			_result.UpdateTileTypes();

			if (_config.DeleteSmallObjects)
			{
				RemoveTiles("small islands", new[] { TileType.Sand, TileType.Land });
				RemoveTiles("small lakes", new[] { TileType.DeepWater, TileType.ShallowWater, TileType.Sand });
				RemoveNoise("water noise", TileType.ShallowWater);

				RemoveTiles("small mountains", TileType.Rock);
				RemoveNoise("mountain noise", TileType.Rock);
				RemoveNoise("snow noise", TileType.Snow);
			}

			// Forests
			GenerateForests();

			RemoveTiles("small forests", TileType.Forest);
			RemoveNoise("forest noise", TileType.Forest);

			// Rivers
			GenerateRivers();
			BuildRiverGroups();
			DigRiverGroups();
		}
	}
}