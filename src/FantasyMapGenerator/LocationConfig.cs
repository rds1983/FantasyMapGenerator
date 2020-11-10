namespace FantasyMapGenerator
{
	public class LocationConfig
	{
		public string Name { get; set; }

		public bool Connected { get; set; }

		public LocationConfig()
		{
			Connected = true;
		}

		public override string ToString()
		{
			return Name + ", " + Connected;
		}
	}
}
