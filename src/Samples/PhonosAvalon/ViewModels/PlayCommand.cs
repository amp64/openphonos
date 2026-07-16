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

        protected readonly GroupViewModel Parent;
        private readonly PlayType Type;

        internal PlayCommand(GroupViewModel parent, PlayType playType)
        {
            Parent = parent;
            Type = playType;
        }

        protected PlayCommand(GroupViewModel parent)
        {
            Parent = parent;
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

        protected virtual Task ExecuteAsync(MusicItem music)
        {
            return Parent.DoAndReportErrors(() =>
            {
                return Parent.Coordinator!.PlayItemAsync(music, Type);
            });
        }
    }

    internal class CustomCommand : PlayCommand
    {
        private readonly Func<Player, MusicItem, Task> CustomAction;

        internal CustomCommand(GroupViewModel parent, Func<Player, MusicItem, Task> action) : base(parent)
        {
            CustomAction = action;
        }

        protected override Task ExecuteAsync(MusicItem music)
        {
            return Parent.DoAndReportErrors(() =>
            {
                return CustomAction(Parent.Coordinator!, music);
            });
        }
    }
}
