using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    internal class GroupVolumeViewModel : IVolume
    {
        private readonly GroupViewModel Group;

        public GroupVolumeViewModel(GroupViewModel group)
        {
            Group = group;
        }

        public int Volume 
        {
            get => Group.GetVolume();
            set
            {
                if (Group.SetVolume(value))
                {
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public bool Muted
        {
            get => Group.GetMuted();
            set
            {
                if (Group.SetMuted(value))
                {
                    OnPropertyChanged(nameof(Muted));
                }
            }
        }

        private bool _Fixed;
        public bool Fixed
        {
            get => _Fixed;

            set
            {
                if (value != _Fixed)
                {
                    _Fixed = value;
                    OnPropertyChanged(nameof(Fixed));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
