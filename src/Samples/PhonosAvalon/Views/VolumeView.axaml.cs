using Avalonia;
using Avalonia.Controls;

namespace PhonosAvalon.Views
{
    public partial class VolumeView : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
                AvaloniaProperty.Register<VolumeView, string>(nameof(Title), defaultValue: string.Empty);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public VolumeView()
        {
            InitializeComponent();
        }
    }
}
