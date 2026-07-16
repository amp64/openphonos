using AsyncImageLoader.Loaders;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PhonosAvalon.ViewModels;
using System;
using OpenPhonos.Sonos;

namespace PhonosAvalon.Views;

public partial class AdaptableView : UserControl
{
    public AdaptableView()
    {
        ImageLoader.AsyncImageLoader.Dispose();
        string path = System.IO.Path.Combine((App.Current as App).DataFilePath, "Cache", "Images");
        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(path);

        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Only NOW is it safe to startup the ViewModel
            _ = vm.StartupAsync();

            // Attach the event
            vm.OnHouseholdNotify += HandleNotification;
        }

        ProgressTimer = new DispatcherTimer();
        ProgressTimer.Interval = TimeSpan.FromSeconds(1);
        ProgressTimer.Tick += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                vm.CurrentZone?.NowPlaying?.UpdateProgress();
            }
        };
        ProgressTimer.Start();

        TopLevel.GetTopLevel(this)!.BackRequested += OnBackRequested;

        base.OnLoaded(e);
    }

    DispatcherTimer NotificationTimer;

    private void HandleNotification(object sender, Household.HouseholdEventArgs args)
    {
        var owner = NotifyControl;

        if (NotificationTimer == null)
        {
            NotificationTimer = new DispatcherTimer();
            NotificationTimer.Interval = TimeSpan.FromSeconds(4);
            NotificationTimer.Tick += (s, e) =>
            {
                owner.IsVisible = false;
                NotificationTimer.Stop();
            };
        }

        NotificationTimer.Stop();

        if (string.IsNullOrEmpty(args.Message))
        {
            owner.IsVisible = false;
        }
        else
        {
            NotifyText.Text = args.Message;
            owner.IsVisible = true;
            NotificationTimer.Start();
        }
    }

    DispatcherTimer ProgressTimer;

    public void ConfigureTimer(bool enable)
    {
        if (ProgressTimer != null)
        {
            ProgressTimer.IsEnabled = enable;
        }
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            e.Handled = vm.OnBackPressed();
        }
    }

    private void Art_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged)
        {
            this.Resources["ArtWidth"] = e.NewSize.Height;
        }
    }

    private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged || e.HeightChanged)
        {
            var w = e.NewSize.Width;
            var h = e.NewSize.Height;
            bool landscape;

            if (w > h)
            {
                landscape = true;
                w = w / 2;
            }
            else
            {
                landscape = false;
                h = h / 2;
            }
            
            double Round(double x)
            {
                // Round to integer and round down to multiple of 4
                return (int)x & ~3;
            }

            this.Resources["Landscape"] = landscape;
            this.Resources["Portrait"] = !landscape;
            this.Resources["PopupWidth"] = Round(Math.Min(w, h));
            this.Resources["PopupHeight"] = e.NewSize.Height;

            this.Resources["LandscapeOnlyWidth"] = landscape ? new GridLength(1, GridUnitType.Star) : new GridLength();
            this.Resources["LandscapeOnlyColumn"] = landscape ? 1 : 0;

            this.Resources["Landscape3else1"] = landscape ? 3 : 1;
            this.Resources["Landscape4else1"] = landscape ? 4 : 1;

            this.Resources["PortraitOnlyWidth"] = !landscape ? new GridLength(1, GridUnitType.Star) : new GridLength();

            // Putting this in App.axaml produces bizarre results for some reason
            this.Resources["ScrollBarSize"] = 6.0;
        }
    }
}
