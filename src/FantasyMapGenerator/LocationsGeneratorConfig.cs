using System.Collections.Generic;

namespace FantasyMapGenerator
{
	public class LocationsGeneratorConfig
	{
		public List<LocationConfig> Locations { get; } = new List<LocationConfig>();

		public LocationsGeneratorConfig()
		{
			Locations.Add(new LocationConfig { Name = "Capital" });
			Locations.Add(new LocationConfig { Name = "Ur" });
			Locations.Add(new LocationConfig { Name = "Wanderers" });
		}
	}
}