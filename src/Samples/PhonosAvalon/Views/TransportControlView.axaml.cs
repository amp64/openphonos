using Avalonia.Controls;
using FluentIcons.Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using PhonosAvalon.ViewModels;
using System.Collections.Generic;

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
        private SymbolIconConverter Converter = new SymbolIconConverter();

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

}
