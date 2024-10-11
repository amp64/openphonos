using Avalonia.Controls;
using FluentIcons.Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using PhonosAvalon.ViewModels;

namespace PhonosAvalon.Views
{
    public partial class TransportControlView : UserControl
    {
        public TransportControlView()
        {
            InitializeComponent();
        }
    }

    public class PlayStateToSymbolIconConverter : IValueConverter
    {
        private SymbolIconConverter Converter = new SymbolIconConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new BindingNotification(null);
            }

            var state = (NowPlayingViewModel.PlayStateType)value;

            switch (state)
            {
                case NowPlayingViewModel.PlayStateType.STOPPED:
                    return Converter.ConvertFrom("Play");
                case NowPlayingViewModel.PlayStateType.TRANSITIONING:
                    return Converter.ConvertFrom("ArrowClockwise");
                case NowPlayingViewModel.PlayStateType.PAUSED_PLAYBACK:
                    return Converter.ConvertFrom("Play");
                case NowPlayingViewModel.PlayStateType.PLAYING:
                    return Converter.ConvertFrom("Pause");
                default:
                    return new BindingNotification(null);
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
