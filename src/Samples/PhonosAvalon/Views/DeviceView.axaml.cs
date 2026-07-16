using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentIcons.Avalonia;
using PhonosAvalon.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using static PhonosAvalon.ViewModels.MainViewModel;

namespace PhonosAvalon.Views;

public partial class DeviceView : UserControl
{
    public DeviceView()
    {
        InitializeComponent();
    }

    private async Task FillHardwareListAsync()
    {
        var vm = DataContext as PhonosAvalon.ViewModels.MainViewModel;
        vm.HouseholdDisplayList = await HardwareItem.CreateHouseholdDisplayAsync(vm.GetHouseholds());
        HardwareTree.SelectedItem = vm.HouseholdDisplayList;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.IsVisible)
        {
            var later = FillHardwareListAsync();
        }
        base.OnAttachedToVisualTree(e);
    }

    private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsVisible = false;
    }
}

internal class SymbolConverter : IValueConverter
{
    private TypeConverter Converter = TypeDescriptor.GetConverter(typeof(SymbolIcon));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        return Converter.ConvertFrom(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
