using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    public class PlayerViewModel : ViewModelBase, IVolume
    {
        private Player ActualPlayer;
        private bool DontSendToDevice;
        private int _Volume;
        private bool _Muted;
        private BatteryViewModel? _BatteryState;        // null if no battery

        public int Volume { get => _Volume; set => SetVolume(value); }
        public bool Muted { get => _Muted; set => SetMuted(value); }
        public bool Fixed { get => ActualPlayer.FixedVolume; set => throw new NotImplementedException(); }
        public string RoomName => ActualPlayer.RoomName;
        public BatteryViewModel? BatteryState { get => _BatteryState; private set => SetProperty(ref _BatteryState, value); }

        // These determine which controls show up on the EQ panel. They never change.
        public bool ShowFixed { get => Fixed; }
        public bool ShowBasic { get => !Fixed; }
        public bool ShowBalance { get => ActualPlayer.Feature3.HasFlag(Player.Feature3Flags.Balance) && !Fixed; }
        public bool ShowHT { get => ActualPlayer.HTCapable && !Fixed; }

        public PlayerViewModel(Player p)
        {
            ActualPlayer = p;
            _Volume = p.DeviceVolume;
            _Muted = p.IsMuted;
            if (p.HasBattery)
            {
                BatteryState = new BatteryViewModel();
            }
            p.PropertyChanged += Player_PropertyChanged;
            ResetCommand = new DoResetCommand(ActualPlayer);
        }

        // These are the names of properties that are monitored in the Player, and have the same named properties here
        private static string[] MonitoredProperties = new string[]
        {
            nameof(Player.Bass),
            nameof(Player.Treble),
            nameof(Player.Loudness),
            nameof(Player.Balance),
            nameof(Player.NightMode),
        };

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.BatteryState))
            {
                BatteryState?.Update(ActualPlayer.BatteryState);
            }
            else if (e.PropertyName == nameof(Player.DialogLevel))
            {
                OnPropertyChanged(nameof(SpeechEnhancement));
            }
            else if (MonitoredProperties.Contains(e.PropertyName))
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        // This is when the UX asks to change it
        private void SetVolume(int newVolume)
        {
            if (SetProperty(ref _Volume, newVolume, nameof(Volume)) && !DontSendToDevice)
            {
                var later = ActualPlayer.SetVolumeAsync(newVolume);
            }
        }

        // This is when the UX asks to change it
        private void SetMuted(bool muted)
        {
            if (SetProperty(ref _Muted, muted, nameof(Muted)) && !DontSendToDevice)
            {
                var later = ActualPlayer.SetMutedAsync(muted);
            }
        }

        // This is when the device has been told of a new value
        public void VolumeHasChanged(int newVolume)
        {
            DontSendToDevice = true;
            Volume = newVolume;
            DontSendToDevice = false;
        }

        // This is when the device has been told of a new value
        public void MutedHasChanged(bool newMute)
        {
            DontSendToDevice = true;
            Muted = newMute;
            DontSendToDevice = false;
        }

        public bool IsPlayer(Player p)
        {
            return ActualPlayer == p;
        }

        public bool Loudness
        {
            get
            {
                return ActualPlayer.Loudness != 0;
            }
            set
            {
                if (value != (ActualPlayer.Loudness != 0))
                {
                    var later = ActualPlayer.SetLoudnessAsync(value);
                    OnPropertyChanged(nameof(Loudness));
                }
            }
        }

        // Treble range is +/-10
        public short Treble
        {
            get
            {
                return ActualPlayer.Treble;
            }
            set
            {
                if (value != ActualPlayer.Treble)
                {
                    var later = ActualPlayer.SetTrebleAsync(value);
                    OnPropertyChanged(nameof(Treble));
                }
            }
        }

        // Bass range is +/-10
        public short Bass
        {
            get
            {
                return ActualPlayer.Bass;
            }
            set
            {
                if (value != ActualPlayer.Bass)
                {
                    var later = ActualPlayer.SetBassAsync(value);
                    OnPropertyChanged(nameof(Bass));
                }
            }
        }

        public bool NightMode
        {
            get
            {
                return ActualPlayer.NightMode;
            }
            set
            {
                if (value != ActualPlayer.NightMode)
                {
                    var later = ActualPlayer.SetNightModeAsync(value);
                    OnPropertyChanged(nameof(NightMode));
                }
            }
        }

        public bool SpeechEnhancement
        {
            get
            {
                return ActualPlayer.DialogLevel != 0;
            }
            set
            {
                if (value != (ActualPlayer.DialogLevel != 0))
                {
                    var later = ActualPlayer.SetDialogLevelAsync((short)(value ? 1 : 0));
                    OnPropertyChanged(nameof(SpeechEnhancement));
                }
            }
        }

        public ICommand ResetCommand { get; private set; }

        private class DoResetCommand(Player player) : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                var later = player.ResetEQAsync();
            }
        }

        // Balance range is +/-100
        public short Balance
        {
            get
            {
                return ActualPlayer.Balance;
            }
            set
            {
                if (value != ActualPlayer.Balance)
                {
                    var later = ActualPlayer.SetBalanceAsync(value);
                    OnPropertyChanged(nameof(Balance));
                }
            }
        }
    }
}
