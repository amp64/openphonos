using Avalonia;
using Avalonia.Controls;
using PhonosAvalon.ViewModels;

namespace PhonosAvalon.Views
{
    public partial class VolumeView : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
                AvaloniaProperty.Register<VolumeView, string>(nameof(Title), defaultValue: string.Empty);

        public static readonly StyledProperty<BatteryViewModel?> BatteryProperty =
                AvaloniaProperty.Register<VolumeView, BatteryViewModel?>(nameof(Battery), defaultValue: null);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public BatteryViewModel? Battery
        {
            get => GetValue(BatteryProperty);
            set => SetValue(BatteryProperty, value);
        }

        public VolumeView()
        {
            InitializeComponent();
        }
    }
}
