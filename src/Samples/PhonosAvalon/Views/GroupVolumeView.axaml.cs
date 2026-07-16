using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using OpenPhonos.Sonos;
using PhonosAvalon.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace PhonosAvalon.Views
{
    public partial class GroupVolumeView : UserControl
    {
        public static readonly StyledProperty<PlayerViewModel?> CurrentEQProperty =
                AvaloniaProperty.Register<GroupVolumeView, PlayerViewModel?>(nameof(CurrentEQ), defaultValue: null);

        public PlayerViewModel? CurrentEQ
        {
            get => GetValue(CurrentEQProperty);
            set => SetValue(CurrentEQProperty, value);
        }

        public GroupVolumeView()
        {
            InitializeComponent();
        }

        public void CloseButton_Clicked(object sender, TappedEventArgs args)
        {
            IsVisible = false;
        }

        public void EQButton_Clicked(object sender, RoutedEventArgs args)
        {
            if (sender is Button button)
            {
                if (button.DataContext is PlayerViewModel player)
                {
                    if (CurrentEQ == player)
                    {
                        // Hide it
                        CurrentEQ = null;
                    }
                    else
                    {
                        // Show it
                        CurrentEQ = player;
                    }
                }
            }
        }
    }

    public class AllSameConverter : IMultiValueConverter
    {
        // If all args are the same, AND not null, return true
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
            {
                return new BindingNotification(null);
            }

            return AllSame(values);
        }

        internal static bool AllSame(IList<object?> values)
        {
            object first = values[0];
            foreach (var v in values)
            {
                if (v != first)
                    return false;
            }

            return first != null;
        }
    }

    // Similar to AllSameConverter but returns icons
    public class AllSameConverterToIcon : IMultiValueConverter
    {
        private TypeConverter _converter = TypeDescriptor.GetConverter(typeof(SymbolIcon));

        public string? TrueIcon { get; set; }
        public string? FalseIcon { get; set; }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
            {
                return new BindingNotification(null);
            }

            bool same = AllSameConverter.AllSame(values);
            string? which = same ? TrueIcon : FalseIcon;
            return _converter.ConvertFrom(which!);
        }
    }
}
