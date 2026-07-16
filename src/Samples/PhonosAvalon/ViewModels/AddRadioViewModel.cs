using OpenPhonos.Sonos;
using System;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    public class AddRadioViewModel : ViewModelBase
    {
        private string _StatusMessage = string.Empty;
        public string StatusMessage
        {
            get => _StatusMessage;
            set => SetProperty(ref _StatusMessage, value);
        }

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        private string _Url = string.Empty;
        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        bool _Busy;
        public bool Busy
        {
            get => _Busy;
            set => SetProperty(ref _Busy, value);
        }

        private Player _Player;

        public AddRadioViewModel(Player player)
        {
            _Player = player;
        }

        internal async Task TestStreamAsync()
        {
            var stream = new RadioStream(Url);
            Busy = true;
            StatusMessage = "Testing...";
            bool ok = await stream.TestStreamAsync(_Player);
            StatusMessage = ok ? "Test passed" : stream.FailureReason;
            Busy = false;
        }

        internal async Task<bool> AddAsync()
        {
            var stream = new RadioStream(Url);
            Busy = true;
            StatusMessage = "Adding...";
            bool ok = false;
            try
            {
                await stream.AddStreamAsync(_Player, Name);
                ok = true;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            Busy = false;
            return ok;
        }
    }
}
