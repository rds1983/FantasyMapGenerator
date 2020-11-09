using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace FantasyMapGenerator.App.UI
{
	public class MapView : Widget
	{
		private static readonly Dictionary<WorldMapTileType, Color> _tilesColors = new Dictionary<WorldMapTileType, Color>
		{
			[WorldMapTileType.Water] = Color.Blue,
			[WorldMapTileType.Land] = Color.Green,
			[WorldMapTileType.Forest] = Color.DarkGreen,
			[WorldMapTileType.Mountain] = Color.Gray,
			[WorldMapTileType.Wall] = Color.RosyBrown,
			[WorldMapTileType.Road] = Color.SaddleBrown
		};

		private GenerationResult _map;
		private Texture2D _texture;

		public GenerationResult Map
		{
			get
			{
				return _map;
			}

			set
			{
				if (value == _map)
				{
					return;
				}

				_map = value;
				_texture = null;
			}
		}

		public bool IgnoreFov;

		public MapView()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			UpdateTexture();

			if (_texture != null)
			{
				context.Batch.Draw(_texture, ActualBounds.Location.ToVector2(), Color.White);
			}
		}

		private void UpdateTexture()
		{
			if (_texture != null || Map == null)
			{
				return;
			}

			var data = new Color[ActualBounds.Width * ActualBounds.Height];
			for(var y = 0; y < ActualBounds.Height; ++y)
			{
				for(var x = 0; x < ActualBounds.Width; ++x)
				{
					var mapX = x * Map.Width / ActualBounds.Width;
					var mapY = y * Map.Height / ActualBounds.Height;

					var tile = Map[mapX, mapY];
					var color = _tilesColors[tile];

					data[y * ActualBounds.Width + x] = color;
				}
			}

			_texture = new Texture2D(MyraEnvironment.GraphicsDevice, ActualBounds.Width, ActualBounds.Height);
			_texture.SetData(data);
		}

		public override void Arrange()
		{
			base.Arrange();

			_texture = null;
		}
	}
}
