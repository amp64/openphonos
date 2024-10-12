using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public PlayerViewModel(Player p)
        {
            ActualPlayer = p;
            _Volume = p.DeviceVolume;
            _Muted = p.IsMuted;
            if (p.HasBattery)
            {
                BatteryState = new BatteryViewModel();
                p.PropertyChanged += Player_PropertyChanged;
            }
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.BatteryState))
            {
                BatteryState?.Update(ActualPlayer.BatteryState);
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
    }
}
