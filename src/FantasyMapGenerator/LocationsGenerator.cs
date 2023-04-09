using GoRogue;
using GoRogue.Pathing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FantasyMapGenerator
{
	public class LocationsGenerator
	{
		private GenerationResult _result;
		private GenerationConfig _config;
		private readonly HashSet<int> _roadTiles = new HashSet<int>();

		public LocationsGenerator(GenerationConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			_config = config;
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


		private void AddToRoadTiles(Point p)
		{
			var h = p.X + (p.Y * _result.Width);

			_roadTiles.Add(h);
		}

		private void Connect(LocationInfo source, LocationInfo dest)
		{
			ReportNextStep($"Building road beetween '{source.Config.Name}' and '{dest.Config.Name}'...");

			AddToRoadTiles(source.Position);

			// Find closest location
			float? closestD = null;
			var startPos = source.Position;
			var destPos = dest.Position;

			foreach (var h in _roadTiles)
			{
				var p = new Point(h % _result.Width, h / _result.Width);
				var d = MathHelper.Distance(p, destPos);

				if (closestD == null || closestD.Value > d)
				{
					closestD = d;
					startPos = p;
				}
			}

			var pathFinder = new AStar(_result, Distance.EUCLIDEAN);

			var path = pathFinder.ShortestPath(startPos, destPos);


			if (path == null || path.Steps == null)
			{
				return;
			}

			foreach (var step in path.Steps)
			{
				_result[step.X, step.Y].TileType = TileType.Road;
				AddToRoadTiles(step);
			}
		}

		public void Generate(GenerationResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException(nameof(result));
			}

			_result = result;

			if (_config.Locations.Count == 0)
			{
				return;
			}

			var pathSet = new bool[_result.Height, _result.Width];
			var areas = new List<Point>();

			// Draw cities
			for (var i = 0; i < _config.Locations.Count; ++i)
			{
				var locationConfig = _config.Locations[i];

				ReportNextStep($"Generating location {locationConfig.Name}...");

				// Generate city location
				var newPoint = Point.Empty;
				int left, top;
				int tries = 100;
				while (tries > 0)
				{
					regenerate:
					tries--;

					var rnd = MathHelper.Random.Next(0, _result.Width - 1);
					left = rnd;

					rnd = MathHelper.Random.Next(0, _result.Height - 1);
					top = rnd;

					if (!_result.IsPassable(left, top))
					{
						goto regenerate;
					}

					// And doesn't intersects with already generated cities
					newPoint.X = left;
					newPoint.Y = top;

					foreach (var r in areas)
					{
						if (MathHelper.Distance(newPoint, r) < 50)
						{
							goto regenerate;
						}
					}

					break;
				}

				if (tries == 0) return;

				// Save area
				areas.Add(newPoint);

				var location = new LocationInfo(locationConfig)
				{
					Position = newPoint
				};

				_result[newPoint].TileType = TileType.Road;
				_result.Locations.Add(location);
			}

			_roadTiles.Clear();
			for (var i = 0; i < _result.Locations.Count - 1; ++i)
			{
				Connect(_result.Locations[i], _result.Locations[i + 1]);
			}
		}
	}
}