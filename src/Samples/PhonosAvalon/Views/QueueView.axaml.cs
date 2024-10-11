using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenPhonos.Sonos;
using PhonosAvalon.ViewModels;
using System;

namespace PhonosAvalon.Views;

public partial class QueueView : UserControl
{
    public QueueView()
    {
        InitializeComponent();

        this.DataContextChanged += OnDataContextChanged;
    }

    // When the CurrentZone changes, reset the list to the top
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        QueueListBox.ScrollIntoView(0);
    }

    private void BackButton_Tapped(object? sender, TappedEventArgs e)
    {
        IsVisible = false;
    }

    private void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Tapping on an item Selects it, which Plays it
        var listbox = sender as ListBox;
        if (e.AddedItems.Count > 0) 
        {
            var item = e.AddedItems[0] as QueueMusicItem;
            var vm = listbox.DataContext as GroupViewModel;
            vm.Queue.QueueItemClicked(item, listbox.SelectedIndex);
            listbox.SelectedIndex = -1;
        }
    }
}