using Myra.Extended.Widgets;
using Myra.Graphics2D;
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
		private GenerationConfig _config;
		private MapView _mapView;
		private LogView _logView;
		private readonly List<Action> _uiThreadActions = new List<Action>();
		private AutoResetEvent _uiEvent = new AutoResetEvent(false);
		private AutoResetEvent _stepEvent = new AutoResetEvent(false);
		private bool _continousRun, _running = false;

		public MainForm()
		{
			BuildUI();

			_config = new GenerationConfig
			{
				LogCallback = LogMessage
			};

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

			_buttonRun.Click += (s, a) =>
			{
				_continousRun = true;
				Task.Factory.StartNew(GenerateTask);
			};

			_buttonStep.Click += (s, a) =>
			{
				if (!_running)
				{
					_continousRun = false;
					Task.Factory.StartNew(GenerateTask);
				}
				else
				{
					ExecuteAtUIThread(() =>
					{
						_buttonStep.Enabled = false;
					});

					_stepEvent.Set();
				}
			};

			_config.NextStepCallback = (s) =>
			{
				_mapView.EraseTexture();

				if (!_continousRun)
				{
					ExecuteAtUIThread(() =>
					{
						_buttonStep.Text = "Step: " + s;
						_buttonStep.Enabled = true;
					});

					_stepEvent.WaitOne();
				}
			};
		}

		public void LogMessage(string message)
		{
			ExecuteAtUIThread(() =>
			{
				_logView.Log(message);
			});
		}

		private void _buttonRun_Click(object sender, EventArgs e)
		{
			Task.Factory.StartNew(GenerateTask);
		}

		private void GenerateTask()
		{
			try
			{
				_running = true;
				ExecuteAtUIThread(() =>
				{
					_buttonRun.Enabled = false;
					_buttonStep.Enabled = false;
					_logView.ClearLog();
					_panelLog.Visible = true;
				});

				var landGenerator = new LandGenerator(_config);
				_mapView.Map = landGenerator.Result;

				landGenerator.Generate();
				var locationsGenerator = new LocationsGenerator(_config);
				locationsGenerator.Generate(landGenerator.Result);
			}
			finally
			{
				_running = false;
				ExecuteAtUIThread(() =>
				{
					_mapView.EraseTexture();
					_buttonRun.Enabled = true;
					_buttonStep.Text = "Step";
					_buttonStep.Enabled = true;
					_panelLog.Visible = false;
				});
			}
		}

		private void ExecuteAtUIThread(Action action)
		{
			lock (_uiThreadActions)
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
				foreach (var action in _uiThreadActions)
				{
					action();
				}

				_uiThreadActions.Clear();
				_uiEvent.Set();
			}
		}
	}
}