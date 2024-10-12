using System.Diagnostics;
using static OpenPhonos.Sonos.Player;

namespace PhonosAvalon.ViewModels
{
    public class BatteryViewModel : ViewModelBase
    {
        private uint _Charged;
        public uint Charged 
        { 
            get => _Charged;
            set
            {
                if (SetProperty(ref _Charged, value))
                {
                    OnPropertyChanged(nameof(Summary));
                }
            }
        }

        private bool _Charging;
        public bool Charging
        {
            get => _Charging;
            set
            {
                if (SetProperty(ref _Charging, value))
                {
                    OnPropertyChanged(nameof(Summary));
                }
            }
        }

        public string Summary
        {
            get
            {
                string result = $"{Charged}% ";
                result += Charging ? "charging" : "charged";
                return result;
            }
        }

        public BatteryViewModel()
        {
            Charged = 50;
            Charging = false;
        }

        public void Update(BatteryStatus status)
        {
            Charging = status.Charging;
            Charged = status.Percent;
        }
    }
}
