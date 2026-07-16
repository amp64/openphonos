using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PhonosAvalon.Views;

public partial class MessageView : UserControl
{
    public MessageView()
    {
        InitializeComponent();
    }

    public void OK_Clicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is PhonosAvalon.ViewModels.MainViewModel vm)
        {
            vm.MessagePopupOpen = false;
        }
    }
}