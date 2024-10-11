using OpenPhonos.Sonos;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    internal class PlayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private Player Player;
        private PlayType Type;

        internal PlayCommand(Player player, PlayType playType)
        {
            Player = player;
            Type = playType;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var music = parameter as MusicItem;
            if (music != null)
            {
                var later = ExecuteAsync(music);
            }
        }

        private async Task ExecuteAsync(MusicItem music)
        {
            try
            {
                await Player.PlayItemAsync(music, Type);
            }
            catch (Exception ex)
            {
                // TODO report this to the user somehow
            }
        }
    }
}