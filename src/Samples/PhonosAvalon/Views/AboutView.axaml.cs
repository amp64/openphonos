using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace PhonosAvalon.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs args)
    {
        IsVisible = false;
    }
}