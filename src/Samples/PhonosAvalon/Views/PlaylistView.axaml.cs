using Avalonia.Controls;
using Avalonia.Input;
using OpenPhonos.Sonos;
using PhonosAvalon.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PhonosAvalon.Views;

public partial class PlaylistView : UserControl
{
    public PlaylistView()
    {
        InitializeComponent();
    }

    private void BackButton_Tapped(object? sender, TappedEventArgs e)
    {
        IsVisible = false;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (args.AddedItems?.Count > 0)
        {
            if (args.AddedItems[0] is MusicItem item)
            {
                _ = SaveAsync(item);
            }
        }
    }

    private async Task SaveAsync(MusicItem item)
    {
        var vm = DataContext as MusicPickerViewModel;
        if (vm.Busy)
        {
            return;
        }

        try
        {
            vm.Playlists.StatusMessage = "Adding...";
            vm.Playlists.Busy = true;
            await vm.Playlists.SaveToPlaylistAsync(item);
            IsVisible = false;
        }
        catch (Exception ex)
        {
            PlaylistListBox.SelectedItem = null;
            vm.Playlists.StatusMessage = ex.Message;
        }
        vm.Playlists.Busy = false;
    }
}