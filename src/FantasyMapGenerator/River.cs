﻿using System.Collections.Generic;

namespace FantasyMapGenerator
{
	public enum RiverDirection
	{
		Left,
		Right,
		Top,
		Bottom
	}

	public class River
	{

		public int Length;
		public List<Tile> Tiles;
		public int ID;

		public int Intersections;
		public float TurnCount;
		public RiverDirection CurrentDirection;

		public River(int id)
		{
			ID = id;
			Tiles = new List<Tile>();
		}

		public void AddTile(Tile tile)
		{
			tile.SetRiverPath(this);
			Tiles.Add(tile);
		}
	}

	public class RiverGroup
	{
		public List<River> Rivers = new List<River>();
	}
}
