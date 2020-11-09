﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace FantasyMapGenerator
{
	public class LocationInfo
	{
		private readonly List<Point> _entranceLocations = new List<Point>();

		public LocationConfig Config { get; private set; }

		public List<Point> EntranceLocations
		{
			get
			{
				return _entranceLocations;
			}
		}

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