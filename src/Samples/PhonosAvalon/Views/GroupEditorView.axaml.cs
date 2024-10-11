using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PhonosAvalon.ViewModels;
using System.Threading.Tasks;
using System.Xml;

namespace PhonosAvalon.Views
{
    public partial class GroupEditorView : UserControl
    {
        public GroupEditorView()
        {
            InitializeComponent();
        }

        public void Button_Ok(object sender, TappedEventArgs args)
        {
            var button = sender as Button;
            var vm = button.DataContext as GroupEditorViewModel;
            this.Cursor = new Cursor(StandardCursorType.Wait);

            vm.OnOKAsync().ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(async () =>
                {
                    this.Cursor = Cursor.Default;

                    if (t.Exception != null)
                    {
                        vm.StatusMessage = t.Exception.Message;
                        await Task.Delay(2000);
                    }

                    Close();
                });
                   
            });
        }

        private void Close()
        {
            var vm = DataContext as GroupViewModel;
            vm.UpdateGroupEditor(true);

            IsVisible = false;
        }

        public void Button_Cancel(object sender, TappedEventArgs args)
        {
            Close();
        }
    }
}
