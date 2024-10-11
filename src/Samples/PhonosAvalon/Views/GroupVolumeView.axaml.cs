using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace PhonosAvalon.Views
{
    public partial class GroupVolumeView : UserControl
    {
        public GroupVolumeView()
        {
            InitializeComponent();
        }

        public void CloseButton_Clicked(object sender, TappedEventArgs args)
        {
            IsVisible = false;
        }
    }
}
