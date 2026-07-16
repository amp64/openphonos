using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using OpenPhonos.Sonos;
using PhonosAvalon.ViewModels;
using System;
using System.Linq;

namespace PhonosAvalon.Views;

public partial class QueueView : UserControl
{
    private int DeleteButtonWidth = 32 + 8;

    public QueueView()
    {
        InitializeComponent();

        this.DataContextChanged += OnDataContextChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.IsVisible)
        {
            if (this.DataContext is GroupViewModel vm)
            {
                vm.Queue.ResetEditState();
            }
        }
        else
        {
            activeTransform = null;
        }
            
        base.OnAttachedToVisualTree(e);
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

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            if (e.AddedItems[0] is UPnPMusicItem item && item.Id == QueueViewModel.NewId)
            {
                var vm = DataContext as GroupViewModel;
                // If New... is selected, switch to Enter-Name mode
                vm.Queue.NewPlaylistName = "";
                vm.Queue.EditState = QueueViewModel.EState.PreSaveNew;
            }
        }
    }

    void DeleteQueue_Click(object sender, RoutedEventArgs e)
    {
        activeTransform = null;

        var button = sender as Button;
        var item = button.DataContext as QueueMusicItem;

        var vm = DataContext as GroupViewModel;
        var listboxitem = button.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        var which = QueueListBox.IndexFromContainer(listboxitem);

        vm.Queue.DeleteQueueItem(item, which);
    }

    #region Swipe-Left-to-Delete

    private Point startPoint;
    private TranslateTransform? activeTransform;
    private bool isDragging;

    private Button GetDeleteButton(object sender)
    {
        var parent = (sender as Border).Parent as Grid;
        var deleteButton = parent.Children.OfType<Button>().First();
        return deleteButton;
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        startPoint = e.GetPosition(null);

        if (sender is Border border && border.RenderTransform is TranslateTransform transform)
        {
            if (GetDeleteButton(sender).IsVisible)
            {
                // We are showing it already, so this might be a drag-right to close it
            }
            else
            {
                // Clear any previous drag state
                if (activeTransform != null)
                    activeTransform.X = 0;
            }

            isDragging = true;
            activeTransform = transform;
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!isDragging || activeTransform == null) 
            return;

        var current = e.GetPosition(null);
        var deltaX = current.X - startPoint.X;

        // Limit drag to horizontal only, to the left only
        if (deltaX < -5)
        {
            activeTransform.X = deltaX;

            // Find the button that is about to be exposed, make it visible
            // That is left until here to avoid "flashes" of them all when the dialog opens
            var deleteButton = GetDeleteButton(sender);
            deleteButton.IsVisible = true;
        }
        else if (deltaX > 5)
        {
            activeTransform.X = 0;
        }
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        isDragging = false;

        if (activeTransform == null) 
            return;

        // Get the details of the item we clicked/dragged
        var border = sender as Border;
        var deleteButton = (border.Parent as Grid).Children.OfType<Button>().First();
        var item = border.DataContext as QueueMusicItem;
        var listbox = border.GetVisualAncestors().OfType<ListBox>().FirstOrDefault();
        var vm = listbox.DataContext as GroupViewModel;
        var listboxitem = border.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        var which = listbox.IndexFromContainer(listboxitem);

        // Just a click, so it's Play/Pause or Cancel-Delete
        if (activeTransform.X == 0)
        {
            activeTransform = null;
            if (!deleteButton.IsVisible)
            {
                vm.Queue.QueueItemClicked(item, which);
            }
            else
            {
                deleteButton.IsVisible = false;
            }
        }
        else if (activeTransform.X < -DeleteButtonWidth)
        {
            activeTransform.X = -DeleteButtonWidth;
        }
        else
        {
            activeTransform.X = 0;
            activeTransform = null;
            deleteButton.IsVisible = false;
        }

        e.Handled = true;
    }

    #endregion
}