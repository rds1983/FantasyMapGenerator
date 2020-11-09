using Myra.Extended.Widgets;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyMapGenerator.App.UI
{
	public partial class MainForm
	{
		private PropertyGrid _propertyGrid;
		private LandGeneratorConfig _config;
		private MapView _mapView;
		private LogView _logView;
		private readonly List<Action> _uiThreadActions = new List<Action>();
		private AutoResetEvent _uiEvent = new AutoResetEvent(false);

		public MainForm()
		{
			BuildUI();

			_config = new LandGeneratorConfig();
			_propertyGrid = new PropertyGrid
			{
				Object = _config
			};

			_splitPane.SetSplitterPosition(0, 0.25f);
			_panelProperties.Widgets.Add(_propertyGrid);

			_mapView = new MapView();
			_panelMap.Widgets.Add(_mapView);

			_logView = new LogView();
			_panelLog.Widgets.Add(_logView);
			_panelLog.Visible = false;

			_buttonGenerate.Click += _buttonGenerate_Click;
		}

		public void LogMessage(string message)
		{
			ExecuteAtUIThread(() =>
			{
				_logView.Log(message);
			});
		}

		private void _buttonGenerate_Click(object sender, System.EventArgs e)
		{
			Task.Factory.StartNew(GenerateTask);
		}

		private void GenerateTask()
		{
			try
			{
				ExecuteAtUIThread(() =>
				{
					_buttonGenerate.Enabled = false;
					_logView.ClearLog();
					_panelLog.Visible = true;
				});

				var generator = new LandGenerator(_config);
				var result = generator.Generate();

				_mapView.Map = result;
			}
			finally
			{
				ExecuteAtUIThread(() =>
				{
					_buttonGenerate.Enabled = true;
					_panelLog.Visible = false;
				});
			}
		}

		private void ExecuteAtUIThread(Action action)
		{
			lock(_uiThreadActions)
			{
				_uiThreadActions.Add(action);
			}

			_uiEvent.WaitOne();
		}

		public override void InternalRender(RenderContext batch)
		{
			base.InternalRender(batch);

			lock (_uiThreadActions)
			{
				foreach(var action in _uiThreadActions)
				{
					action();
				}

				_uiThreadActions.Clear();
				_uiEvent.Set();
			}
		}
	}
}