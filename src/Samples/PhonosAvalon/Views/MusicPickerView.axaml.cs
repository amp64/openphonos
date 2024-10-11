using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using OpenPhonos.Sonos;
using PhonosAvalon.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace PhonosAvalon.Views;

public partial class MusicPickerView : UserControl
{
    public MusicPickerView()
    {
        InitializeComponent();

        this.DataContextChanged += OnDataContextChanged;
    }

    // When the CurrentZone changes, may need to update the MusicPicker
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        var element = sender as MusicPickerView;
        var group = element.DataContext as GroupViewModel;
        if (group != null && group.MusicSource != null)
        {
            group.MusicSource.SourcePlayer = group.Coordinator;
        }
    }

#if false
    public GroupViewModel Group
    {
        get => this.GetValue(GroupProperty) as GroupViewModel;
        set => this.SetValue(GroupProperty, value);
    }

    public static readonly AttachedProperty<GroupViewModel> GroupProperty =
        AvaloniaProperty.RegisterAttached<MusicPickerView,
            MusicPickerView, GroupViewModel>(
            nameof(Group),
            null,
            false,
            Avalonia.Data.BindingMode.OneWay,
            GroupValidator);

    private static bool GroupValidator(GroupViewModel model)
    {
        return true;
    }
#endif

    public void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        var listbox = sender as ListBox;
        var vm = listbox.DataContext as MusicPickerViewModel;
        if (args.AddedItems?.Count > 0)
        {
            listbox.SelectedItem = null;

            if (args.AddedItems[0] is MusicItem music)
            {
                if (music.IsContainer)
                {
                    vm.MusicItemClicked(music, listbox);
                }
                else if (music.IsPlayable)
                {
                    if (DataContext is GroupViewModel group && group.PlayNowCommand.CanExecute(music))
                    {
                        group.PlayNowCommand.Execute(music);
                    }
                }
            }
        }
    }

    // must match XAML value
    const double ListItemPadding = 4;

    public void WrapPanel_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        var grid = sender as Grid;
        if (Resources.TryGetValue("GridItemWidth", out var w))
        {
            double itemWidth = (double)w;
            const double pad = ListItemPadding * 2;
            var items = args.NewSize.Width / (itemWidth + pad);
            if (items < 2.0)
            {
                // Ensure we can always fit two Grid items
                double newWidth = args.NewSize.Width / 2 - pad;
                Resources["GridItemWidth"] = newWidth;
                Resources["GridItemHeight"] = newWidth + ((GridLength)Resources["GridItemTextHeight"]).Value;
                Resources["GridItemArtHeight"] = new GridLength(newWidth);
            }
        }
    }

    /// <summary>
    /// Make a click on an item open the context menu (instead of just a right-click)
    /// </summary>
    /// <param name="sender">Control that was clicked</param>
    /// <param name="args">TappedEventArgs</param>
    public void PlayButton_Clicked(object sender, TappedEventArgs args)
    {
        var control = sender as Control;
        var context = control.ContextMenu;
        if (context.IsOpen)
        {
            context.Close();
        }
        else
        {
            // Build the menu based on the item
            var group = DataContext as GroupViewModel;
            var music = control.DataContext as MusicItem;

            context.Items.Clear();
            context.Items.Add(new MenuItem() { Header = "Play Now", Command = group.PlayNowCommand, CommandParameter = music });
            if (music.IsQueueable)
            {
                context.Items.Add(new MenuItem() { Header = "Play Next", Command = group.PlayNextCommand, CommandParameter = music });
                context.Items.Add(new MenuItem() { Header = "Add to End of Queue", Command = group.AddToQueueCommand, CommandParameter = music });
                context.Items.Add(new MenuItem() { Header = "Replace Queue", Command = group.ReplaceQueueCommand, CommandParameter = music });
            }
            if (music.IsDeletable)
            {
                context.Items.Add(new MenuItem() { Header = "Delete", IsEnabled = false });
            }
            if (music.IsFavoritable)
            { 
                context.Items.Add(new MenuItem() { Header = "Add to Sonos Favorites", IsEnabled = false });
            }
            if (false)
            {
                // TODO
                context.Items.Add(new MenuItem() { Header = "Add to Sonos Playlist" , IsEnabled = false });
            }
            if (false)
            {
                // TODO
                context.Items.Add(new MenuItem() { Header = "Add to <Music Service> Playlist" , IsEnabled = false });
            }
            context.Open();
        }
    }

    public void SearchTextBox_KeyUp(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter && sender is TextBox control)
        {
            DoSearch(control);
        }
    }

    public void SearchTextBox_Search(object sender, TappedEventArgs args)
    {
        DoSearch(SearchTextBox);
    }

    private void DoSearch(TextBox control)
    {
        var vm = control.DataContext as MusicPickerViewModel;
        var cmd = vm.SearchCommand;
        string arg = control.Text;
        if (cmd.CanExecute(arg))
        {
            cmd.Execute(arg);
        }
    }
}

public class ViewTypeConverter : IValueConverter
{
    // Returns true if the value (as string) matches the Parameter
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = value?.ToString();
        var param = parameter?.ToString();
        return val == param;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SafeGetItemConverter : IValueConverter
{
    // Passed an IList and return the [parameter] item, if possible, else null
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var list = value as IList;
        int index = int.Parse((string)parameter);
        if (index < list.Count)
        {
            return list[index];
        }
        else
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringSameConverter : IValueConverter
{
    public string? DefaultValue { get; set; }

    // if value == parameter, or value == null and parameter == DefaultValue, return true
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? a = value?.ToString() ?? DefaultValue;
        string b = parameter?.ToString() ?? "";
        return a == b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// based on https://stackoverflow.com/questions/63207058/selecting-a-datatemplate-based-on-datacontext-property-in-avalonia
public class MusicTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();

    public Control? Build(object? data)
    {
        string name = "Default";
        var music = data as MusicItem;
        if (music?.InlineChildren != null)
        {
            name = "Mixed";
        }
        return Templates[name].Build(data);
    }

    public bool Match(object? data)
    {
        return data is MusicItem;
    }
}
