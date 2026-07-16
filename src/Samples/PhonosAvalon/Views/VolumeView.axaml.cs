using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PhonosAvalon.ViewModels;
using System;

namespace PhonosAvalon.Views
{
    public partial class VolumeView : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
                AvaloniaProperty.Register<VolumeView, string>(nameof(Title), defaultValue: string.Empty);

        public static readonly StyledProperty<BatteryViewModel?> BatteryProperty =
                AvaloniaProperty.Register<VolumeView, BatteryViewModel?>(nameof(Battery), defaultValue: null);

        public static readonly StyledProperty<bool> ShowNumberProperty =
                AvaloniaProperty.Register<VolumeView, bool>(nameof(ShowNumber), defaultValue: false);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public BatteryViewModel? Battery
        {
            get => GetValue(BatteryProperty);
            set => SetValue(BatteryProperty, value);
        }

        public bool ShowNumber
        {
            get => GetValue(ShowNumberProperty);
            set => SetValue(ShowNumberProperty, value);
        }

        private readonly DispatcherTimer _timer;

        public VolumeView()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object? sender, System.EventArgs e)
        {
            _timer.Stop();
            ShowNumber = false;
        }

        private void RestartTimer()
        {
            ShowNumber = true;
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Start();
        }

        private void OnValueChanged(object sender, RoutedEventArgs args)
        {
            RestartTimer();
        }

        public void VolumeDown_Click(object sender, RoutedEventArgs args)
        {
            VolumeChange(-1);
        }

        public void VolumeUp_Click(object sender, RoutedEventArgs args)
        {
            VolumeChange(+1);
        }

        private void VolumeChange(int delta)
        {
            double newValue = PART_Slider.Value;
            double step = (newValue < 35) ? 2 : 5;
            newValue += (delta * step);
            newValue = double.Max(newValue, PART_Slider.Minimum);
            newValue = double.Min(newValue, PART_Slider.Maximum);
            PART_Slider.Value = newValue;
        }
    }
}
