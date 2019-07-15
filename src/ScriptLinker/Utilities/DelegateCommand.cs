using System;
using System.Windows.Input;

namespace ScriptLinker.Utilities
{
    /// <summary>
    /// A reusable ICommand
    /// https://www.wpftutorial.net/DelegateCommand.html
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action<object> execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute) : this(execute, null)
        {

        }

        public DelegateCommand(Action<object> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object param)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute();
        }

        public void Execute(object param)
        {
            execute(param);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
