using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Monitor2ng.Models
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> RelayedExecute { get; set; }

        public delegate bool RelayedCanExecuteHandler (object parameter);
        public RelayedCanExecuteHandler RelayedCanExecute { get; set; }

        public RelayCommand(Action<object> execute, RelayedCanExecuteHandler canExecute)
        {
            RelayedExecute = execute;
            RelayedCanExecute = canExecute;
        }

        public RelayCommand(Action<object> execute)
        {
            RelayedExecute = execute;
            RelayedCanExecute = (object parameter) => { return true; };
        }

        public bool CanExecute(object parameter)
        {
            return RelayedCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            RelayedExecute(parameter);
        }
    }
}
