using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using PhonosAvalon.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace PhonosAvalon.Views;

public partial class AddRadioView : UserControl
{
    public AddRadioView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.IsVisible)
        {
            var vm = DataContext as MainViewModel;
            vm.RadioViewModel = new AddRadioViewModel(vm.CurrentZone!.Coordinator!);
        }

        base.OnAttachedToVisualTree(e);
    }

    private void Button_Add(object sender, RoutedEventArgs e)
    {
        _ = AddAsync();
    }

    private async Task AddAsync()
    {
        var vm = DataContext as MainViewModel;
        bool ok = await vm.RadioViewModel.AddAsync();
        if (ok)
        {
            IsVisible = false;
        }
    }

    private void Button_Test(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainViewModel;
        _ = vm.RadioViewModel.TestStreamAsync();
    }

    private void Button_Cancel(object sender, RoutedEventArgs e)
    {
        IsVisible = false;
    }
}

internal class IsUrlValidConverter : IValueConverter
{
    // Converts a string into a bool, true only if a valid url
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        string url = (string)value;
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
