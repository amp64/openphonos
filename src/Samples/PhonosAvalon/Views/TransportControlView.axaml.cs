using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentIcons.Avalonia;
using PhonosAvalon.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace PhonosAvalon.Views
{
    public partial class TransportControlView : UserControl
    {
        public TransportControlView()
        {
            InitializeComponent();
        }
    }

    public class PlayStateToSymbolIconConverter : IMultiValueConverter
    {
        private TypeConverter Converter = TypeDescriptor.GetConverter(typeof(SymbolIcon));

        // First binding is PlayState, second binding is CanPause, third is CanStop
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return new BindingNotification(null);
            }

            if (
                (values[0] is not NowPlayingViewModel.PlayStateType) ||
                (values[1] is not bool) ||
                (values[2] is not bool)
               )
            {
                return BindingOperations.DoNothing;
            }

            var state = (NowPlayingViewModel.PlayStateType)values[0];
            bool canPause = (bool)values[1];
            bool canStop = (bool)values[2];

            switch (state)
            {
                case NowPlayingViewModel.PlayStateType.STOPPED:
                    return Converter.ConvertFrom("Play");
                case NowPlayingViewModel.PlayStateType.TRANSITIONING:
                    return Converter.ConvertFrom("ArrowClockwise");
                case NowPlayingViewModel.PlayStateType.PAUSED_PLAYBACK:
                    return Converter.ConvertFrom("Play");
                case NowPlayingViewModel.PlayStateType.PLAYING:
                    return Converter.ConvertFrom(canPause || !canStop ? "Pause" : "Stop");
                default:
                    return new BindingNotification(null);
            }
        }
    }

    // convert a bool to one of two brushes
    public class BoolToBrushConverter : IValueConverter
    {
        public IBrush? TrueBrush { get; set; }
        public IBrush? FalseBrush { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool v = (bool)value;
            IBrush? b = v ? TrueBrush : FalseBrush;
            return b != null ? b : BindingValue<IBrush>.Unset;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converts a bool into a SymbolIcon
    public class BoolToSymbolIconConverter : IValueConverter
    {
        private TypeConverter _converter = TypeDescriptor.GetConverter(typeof(SymbolIcon));

        public string? TrueIcon { get; set; }
        public string? FalseIcon { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var b = value != null && (bool)value;
            string? which = b ? TrueIcon : FalseIcon;

            if (which != null)
            {
                return _converter.ConvertFrom(which);
            }
            else
            {
                return BindingValue<object>.Unset;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RepeatToSymbolIconConverter : IValueConverter
    {
        private TypeConverter _converter = TypeDescriptor.GetConverter(typeof(SymbolIcon));

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var repeat = (NowPlayingViewModel.RepeatMode)value;
            if (repeat == NowPlayingViewModel.RepeatMode.Off)
            {
                return _converter.ConvertFrom("ArrowRepeatAllOff");
            }
            else if (repeat == NowPlayingViewModel.RepeatMode.On)
            {
                return _converter.ConvertFrom("ArrowRepeatAll");
            }
            else if (repeat == NowPlayingViewModel.RepeatMode.One)
            {
                return _converter.ConvertFrom("ArrowRepeat1");
            }
            else
            {
                return BindingValue<object>.Unset;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
