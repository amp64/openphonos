using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;

namespace PhonosAvalon.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs args)
    {
        IsVisible = false;
    }

    private void Network_Click(object sender, RoutedEventArgs args)
    {
        Settings.Instance.Hosts = string.Empty;
        (App.Current as App).TidyExit();
    }

    private void DeleteAll_Click(object sender, RoutedEventArgs args)
    {
        Settings.Instance.Reset();
        (App.Current as App).TidyExit();
    }
}
