using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    public class PlayStateToLabelConverter : IValueConverter
    {
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
                    return "Stopped";
                case NowPlayingViewModel.PlayStateType.TRANSITIONING:
                    return "Wait...";
                case NowPlayingViewModel.PlayStateType.PAUSED_PLAYBACK:
                    return "Play";
                case NowPlayingViewModel.PlayStateType.PLAYING:
                    return "Pause";
                default:
                    return "Unknown";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
