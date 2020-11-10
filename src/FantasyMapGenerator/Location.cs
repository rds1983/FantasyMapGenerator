using System;
using System.Collections.Generic;
using System.Drawing;

namespace FantasyMapGenerator
{
	public class LocationInfo
	{
		public LocationConfig Config { get; private set; }

		public Point Position;

		public LocationInfo(LocationConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			Config = config;
		}
	}
}