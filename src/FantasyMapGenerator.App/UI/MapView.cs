using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Utility;
using System.Collections.Generic;
using System.Linq;

namespace FantasyMapGenerator.App.UI
{
	public class MapView : Widget
	{
		private static readonly Point LocationTextureSize = new Point(32, 32);

		private static readonly Dictionary<WorldMapTileType, Color> _tilesColors = new Dictionary<WorldMapTileType, Color>
		{
			[WorldMapTileType.Water] = Color.Blue,
			[WorldMapTileType.Land] = Color.Green,
			[WorldMapTileType.Forest] = Color.DarkGreen,
			[WorldMapTileType.Mountain] = Color.Gray,
			[WorldMapTileType.HighMountain] = Color.White,
			[WorldMapTileType.Wall] = Color.RosyBrown,
			[WorldMapTileType.Road] = Color.SaddleBrown
		};

		private GenerationResult _map;
		private Texture2D _texture;

		private TextureRegion _locationTexture;

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

		public SpriteFontBase Font = DefaultAssets.UIStylesheet.Fonts.Values.First();

		public MapView()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			var assembly = typeof(MapView).Assembly;

			using (var stream = Res.OpenResourceStream(assembly, "FantasyMapGenerator.App.Resources.overworld.png"))
			{
				var texture = Texture2D.FromStream(MyraEnvironment.GraphicsDevice, stream);
				_locationTexture = new TextureRegion(texture, new Rectangle(272, 128, 16, 16));
			}
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			UpdateTexture();

			if (_texture != null)
			{
				context.Draw(_texture, ActualBounds.Location.ToVector2(), Color.White);
			}

			if (_map != null && _map.Locations != null)
			{
				foreach (var location in _map.Locations)
				{
					var x = location.Position.X * ActualBounds.Width / _map.Width;
					var y = location.Position.Y * ActualBounds.Height / _map.Height;

					var sz = Font.MeasureString(location.Config.Name);

					// Draw name
					context.DrawString(Font, location.Config.Name,
						new Vector2(ActualBounds.X + x - sz.X / 2, ActualBounds.Y + y + LocationTextureSize.Y / 2),
						Color.White);

					// Draw icon
					var rect = new Rectangle(ActualBounds.X + x - LocationTextureSize.X / 2,
						ActualBounds.Y + y - LocationTextureSize.Y / 2,
						LocationTextureSize.X, LocationTextureSize.Y);
					_locationTexture.Draw(context, rect, Color.White);
				}
			}
		}

		private void UpdateTexture()
		{
			if (_texture != null || Map == null)
			{
				return;
			}

			var data = new Color[ActualBounds.Width * ActualBounds.Height];
			for (var y = 0; y < ActualBounds.Height; ++y)
			{
				for (var x = 0; x < ActualBounds.Width; ++x)
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

		public void EraseTexture()
		{
			_texture = null;
		}

		public override void InternalArrange()
		{
			base.InternalArrange();

			_texture = null;
		}
	}
}
