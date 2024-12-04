using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Input;
using Avalonia.VisualTree;
using PhonosAvalon.ViewModels;
using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using DialogHostAvalonia.Positioners;
using Avalonia;

namespace PhonosAvalon.Views;

public partial class MobileMainView : UserControl
{
    private ParametrizedLogger? _Logger;

    public MobileMainView()
    {
        ImageLoader.AsyncImageLoader.Dispose();
        string path = System.IO.Path.Combine((App.Current as App).DataFilePath, "Cache", "Images");
        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(path);

        InitializeComponent();

        DialogHost.PopupPositioner = new DialogPopupPositioner();
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

    private class DialogPopupPositioner : IDialogPopupPositioner
    {
        public Rect Update(Size anchorRectangle, Size size)
        {
            double margin = 12;
            Rect posn = new Rect(0, 0, anchorRectangle.Width, anchorRectangle.Height);
            return posn.Inflate(-margin);
        }
    }
}
