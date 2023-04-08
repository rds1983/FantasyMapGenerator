using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FantasyMapGenerator
{
	public class GenerationConfig
	{
		public int WorldSize { get; set; }
		public int HeightMapVariability { get; set; }
		public float LandPart { get; set; }
		public float MountainPart { get; set; }
		public float ForestPart { get; set; }
		public bool SurroundedByWater { get; set; }
		public bool Smooth { get; set; }
		public bool RemoveSmallIslands { get; set; }
		public bool RemoveSmallLakes { get; set; }

		public List<LocationConfig> Locations { get; } = new List<LocationConfig>();

		[Browsable(false)]
		public Action<string> LogCallback;

		[Browsable(false)]
		public Action MapChangedCallback;

		public GenerationConfig()
		{
			WorldSize = 1024;
			LandPart = 0.6f;
			MountainPart = 0.1f;
			ForestPart = 0.1f;
			HeightMapVariability = 5;
			Smooth = true;
			RemoveSmallIslands = true;
			RemoveSmallLakes = true;
			//			SurroundedByWater = true;

			Locations.Add(new LocationConfig { Name = "Bal Harbor" });
			Locations.Add(new LocationConfig { Name = "Westwood" });
			Locations.Add(new LocationConfig { Name = "Goblin Mountain" });
			Locations.Add(new LocationConfig { Name = "Kobolds Village" });
			Locations.Add(new LocationConfig { Name = "Kuo Toans" });
			Locations.Add(new LocationConfig { Name = "Atlantis" });
			Locations.Add(new LocationConfig { Name = "Wagoneers" });
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("WorldSize=" + WorldSize + ",\n");
			sb.Append("HeightMapVariability=" + HeightMapVariability + ",\n");
			sb.Append("LandPart=" + (int)(LandPart * 100.0f) + "%,\n");
			sb.Append("MountainPart=" + (int)(MountainPart * 100.0f) + "%,\n");
			sb.Append("ForestPart=" + (int)(ForestPart * 100.0f) + "%,\n");
			sb.Append("SurroundedByWater=" + SurroundedByWater + ",\n");
			sb.Append("Smooth=" + Smooth + ",\n");
			sb.Append("RemoveSmallIslands=" + RemoveSmallIslands + ",\n");
			sb.Append("RemoveSmallLakes=" + RemoveSmallLakes);

			return sb.ToString();
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