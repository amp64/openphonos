using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        string path = System.IO.Path.Combine((App.Current as App).DataFilePath, "Cache", "Images");
        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(path);

        InitializeComponent();

        ArtPanel.PropertyChanged += ArtPanel_PropertyChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Only NOW is it safe to startup the ViewModel
            _ = vm.StartupAsync();
        }

        base.OnLoaded(e);
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
