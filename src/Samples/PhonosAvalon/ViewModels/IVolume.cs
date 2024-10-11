using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    public interface IVolume : INotifyPropertyChanged
    {
        int Volume { get; set; }
        bool Muted { get; set; }
        bool Fixed { get; set; }
    }
}
