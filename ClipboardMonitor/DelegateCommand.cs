using System;
using System.Windows.Input;

namespace ClipboardMonitor
{
    public class DelegateCommand : ICommand
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Execute(object parameter) => CommandAction();

        public bool CanExecute(object parameter) => CanExecuteFunc == null || CanExecuteFunc();

        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}