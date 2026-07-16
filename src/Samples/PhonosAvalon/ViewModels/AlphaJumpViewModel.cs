using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    public class AlphaJumpViewModel : ViewModelBase
    {
        public DoJumpCommand JumpCommand { get; private set; }

        private Dictionary<char, int> _Index = new Dictionary<char, int>();

        public event Action<int>? OnJump;

        public AlphaJumpViewModel()
        {
            JumpCommand = new DoJumpCommand(this);
        }

        public void TryAddToIndex(string item, int index)
        {
            if (!string.IsNullOrEmpty(item))
            {
                char alpha = item[0];
                // if its digits then "0", if its alpha then upper case
                if (char.IsDigit(alpha))
                {
                    alpha = '0';
                }
                else
                {
                    alpha = char.ToUpper(alpha);
                }
                if (!_Index.ContainsKey(alpha))
                {
                    _Index.Add(alpha, index);
                    JumpCommand.HasChanged();
                }
            }
        }

        public class DoJumpCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly AlphaJumpViewModel _parent;

            public DoJumpCommand(AlphaJumpViewModel parent)
            {
                _parent = parent;
            }

            public void HasChanged()
            {
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object? parameter)
            {
                if (parameter != null)
                {
                    string alpha = parameter as string;
                    return _parent._Index.ContainsKey(alpha[0]);
                }

                return false;
            }

            public void Execute(object? parameter)
            {
                if (parameter != null)
                {
                    string alpha = parameter as string;
                    int index = _parent._Index[alpha[0]];
                    _parent.OnJump?.Invoke(index);
                }
            }
        }

    }
}
