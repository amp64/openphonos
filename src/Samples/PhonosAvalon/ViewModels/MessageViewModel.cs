using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string? _Message;
        public string? Message
        {
            get => _Message; 
            set => SetProperty(ref _Message, value);
        }

        private string? _LinkMessage;
        public string? LinkMessage
        {
            get => _LinkMessage; 
            set => SetProperty(ref _LinkMessage, value);
        }

        private string? _Link;
        public string? Link
        {
            get => _Link; 
            set => SetProperty(ref _Link, value);
        }
    }
}
