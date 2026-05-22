using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SubEventSystem.Events;

namespace SubEventSystem
{
    public partial class MainWindow : Window
    {
        private readonly SubEvent<SensorReading> _sensorEvent;
        private readonly SubEventToken<SensorReading> _tempToken;
        private readonly SubEventToken<SensorReading> _pressureToken;

        private DispatcherTimer _timer;
        private readonly Random _random = new();
        private int _activeCount;

        public MainWindow()
        {
            InitializeComponent();

            // Setup SubEvent con callbacks di attivazione
            _sensorEvent = new SubEvent<SensorReading>(
                OnFirstListenerActivation: () =>
                {
                    Log("⚡ First listener activated → ready to receive");
                    UpdateListenersLed(true);
                },
                OnLastListenerDeactivation: () =>
                {
                    Log("💤 All listeners deactivated → idle");
                    UpdateListenersLed(false);
                });

            // Subscribe dei widget
            _tempToken = _sensorEvent.Subscribe(OnTemperatureReceived, initialState: true);
            _pressureToken = _sensorEvent.Subscribe(OnPressureReceived, initialState: true);

            UpdateActiveCount();
        }

        // --- Producer ---

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null) return;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            ProducerLed.Fill = new SolidColorBrush(Color.FromRgb(0x4e, 0xc9, 0x4e));
            TxtProducerState.Text = "Producing...";
            Log("▶ Producer started");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_timer == null) return;

            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _timer = null;

            ProducerLed.Fill = new SolidColorBrush(Colors.Gray);
            TxtProducerState.Text = "Stopped";
            Log("⏹ Producer stopped");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var temp = new SensorReading("Temperature", Math.Round(18 + _random.NextDouble() * 15, 1));
            var press = new SensorReading("Pressure", Math.Round(1000 + _random.NextDouble() * 50, 1));

            _sensorEvent.Invoke(temp);
            _sensorEvent.Invoke(press);
        }

        // --- Handlers (consumers) ---

        private void OnTemperatureReceived(SensorReading reading)
        {
            if (reading.SensorName != "Temperature") return;
            TxtTemp.Text = $"{reading.Value} °C";
            Log($"🌡 Temp received: {reading.Value} °C");
        }

        private void OnPressureReceived(SensorReading reading)
        {
            if (reading.SensorName != "Pressure") return;
            TxtPressure.Text = $"{reading.Value} hPa";
            Log($"💨 Pressure received: {reading.Value} hPa");
        }

        // --- Toggle tokens ---

        private void ChkTemp_Changed(object sender, RoutedEventArgs e)
        {
            if (_tempToken == null) return; // non ancora inizializzato

            _tempToken.IsActive = ChkTemp.IsChecked == true;
            Log(_tempToken.IsActive ? "🌡 Temp token ACTIVATED" : "🌡 Temp token DEACTIVATED");

            if (!_tempToken.IsActive) TxtTemp.Text = "-- °C";
            UpdateActiveCount();
        }

        private void ChkPressure_Changed(object sender, RoutedEventArgs e)
        {
            if (_pressureToken == null) return; // non ancora inizializzato

            _pressureToken.IsActive = ChkPressure.IsChecked == true;
            Log(_pressureToken.IsActive ? "💨 Pressure token ACTIVATED" : "💨 Pressure token DEACTIVATED");

            if (!_pressureToken.IsActive) TxtPressure.Text = "-- hPa";
            UpdateActiveCount();
        }

        // --- UI helpers ---

        private void UpdateActiveCount()
        {
            _activeCount = (_tempToken.IsActive ? 1 : 0) + (_pressureToken.IsActive ? 1 : 0);
            TxtActiveListeners.Text = $"Active listeners: {_activeCount}/2";
        }

        private void UpdateListenersLed(bool active)
        {
            ListenersLed.Fill = new SolidColorBrush(active
                ? Color.FromRgb(0x4e, 0xc9, 0x4e)
                : Colors.Gray);
        }

        private void Log(string message)
        {
            LstLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            if (LstLog.Items.Count > 50) LstLog.Items.RemoveAt(LstLog.Items.Count - 1);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _timer?.Stop();
            _sensorEvent.Dispose();
        }
    }

    // --- Model ---

    public record SensorReading(string SensorName, double Value);
}