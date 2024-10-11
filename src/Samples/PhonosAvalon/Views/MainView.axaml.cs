using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using PhonosAvalon.ViewModels;
using System;

namespace PhonosAvalon.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        ImageLoader.AsyncImageLoader.Dispose();
        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader();

        InitializeComponent();

        ArtPanel.PropertyChanged += ArtPanel_PropertyChanged;
    }

    private void ArtPanel_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ArtPanel.Bounds))
        {
            StackPanel panel = sender as StackPanel;
            var vm = this.DataContext as MainViewModel;
            var delta = RoomCombo.Bounds.Height + TrackProgress.Bounds.Height;
            vm.ArtSize = Math.Min(panel.Bounds.Width, panel.Bounds.Height) - delta;
        }
    }
}
