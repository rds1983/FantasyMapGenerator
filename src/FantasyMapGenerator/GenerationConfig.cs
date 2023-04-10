using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FantasyMapGenerator
{
	public class GenerationConfig
	{
		public int WorldSize { get; set; }
		
		[Category("Height Map")]
		public int TerrainOctaves = 6;
		[Category("Height Map")]
		public double TerrainFrequency = 1.25;

		[Category("Height Map")]
		public float DeepWaterPart = 0.1f;
		[Category("Height Map")]
		public float ShallowWaterPart = 0.3f;
		[Category("Height Map")]
		public float SandPart = 0.05f;
		[Category("Height Map")]
		public float LandPart = 0.4f;
		[Category("Height Map")]
		public float RockPart = 0.1f;

		[Category("Forests")]
		public float ForestPart = 0.1f;

		[Category("Rivers")]
		public int RiverCount = 40;
		[Category("Rivers")]
		public float MinRiverHeight = 0.6f;
		[Category("Rivers")]
		public int MaxRiverAttempts = 1000;
		[Category("Rivers")]
		public int MinRiverTurns = 18;
		[Category("Rivers")]
		public int MinRiverLength = 20;
		[Category("Rivers")]
		public int MaxRiverIntersections = 2;

		[Category("Utility")]
		public bool SphericalWorld = true;

		[Category("Utility")]
		public bool DeleteSmallObjects = false;

		public List<LocationConfig> Locations { get; } = new List<LocationConfig>();

		[Browsable(false)]
		public Action<string> LogCallback;

		[Browsable(false)]
		public Action<string> NextStepCallback;

		public GenerationConfig()
		{
			WorldSize = 1024;
			//			SurroundedByWater = true;

			Locations.Add(new LocationConfig { Name = "Bal Harbor" });
			Locations.Add(new LocationConfig { Name = "Westwood" });
			Locations.Add(new LocationConfig { Name = "Goblin Mountain" });
			Locations.Add(new LocationConfig { Name = "Kobolds Village" });
			Locations.Add(new LocationConfig { Name = "Kuo Toans" });
			Locations.Add(new LocationConfig { Name = "Atlantis" });
			Locations.Add(new LocationConfig { Name = "Wagoneers" });
		}

		public void LogInfo(string msg)
		{
			if (LogCallback == null)
			{
				return;
			}

			LogCallback(msg);
		}
	}
}