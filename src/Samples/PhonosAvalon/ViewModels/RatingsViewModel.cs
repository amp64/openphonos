using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    /// <summary>
    /// Use to bind to a Rating button (Like/Dislike etc)
    /// </summary>
    public class RatingsViewModel : ObservableObject
    {
        private bool _Enabled;
        private string _BeforeMessage;
        private string _AfterMessage;
        private string? _Image;
        private Func<int, Task> _Command;
        private int _Index;

        public bool Enabled
        {
            get => _Enabled;
            internal set => SetProperty(ref _Enabled, value);
        }

        public string BeforeMessage
        {
            get => _BeforeMessage;
            internal set => SetProperty(ref _BeforeMessage, value);
        }

        public string AfterMessage
        {
            get => _AfterMessage;
            internal set => SetProperty(ref _AfterMessage, value);
        }

        public string? Image
        {
            get => _Image;
            internal set => SetProperty(ref _Image, value);
        }

        public RatingsViewModel(Func<int, Task> ratingsCommand, int index)
        {
            _Enabled = false;
            _BeforeMessage = string.Empty;
            _AfterMessage = string.Empty;
            _Image = null;
            _Command = ratingsCommand;
            _Index = index;
        }

        public void Reset()
        {
            Enabled = false;
            BeforeMessage = string.Empty;
            AfterMessage = string.Empty;
            Image = null;
        }

        public async Task SetRatingsCommand(string _)
        {
            await _Command.Invoke(_Index);
        }
    }
}
