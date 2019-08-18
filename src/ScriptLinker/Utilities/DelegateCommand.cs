using System;
using System.Windows.Input;

namespace ScriptLinker.Utilities
{
    /// <summary>
    /// A reusable ICommand
    /// https://www.wpftutorial.net/DelegateCommand.html
    /// https://stackoverflow.com/a/6273036/9449426
    /// </summary>
    public class DelegateCommand<TParameter> : ICommand
    {
        protected readonly Func<TParameter, bool> canExecute;
        protected readonly Action<TParameter> execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<TParameter> execute) : this(execute, null)
        {

        }

        public DelegateCommand(Action<TParameter> execute, Func<TParameter, bool> canExecute)
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

            var castParam = (TParameter)Convert.ChangeType(param, typeof(TParameter));
            return canExecute(castParam);
        }

        public void Execute(object param)
        {
            var castParam = (TParameter)Convert.ChangeType(param, typeof(TParameter));
            execute(castParam);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action execute) : this(execute, null)
        {
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
            : base(
                  (object o) => execute(),
                  (object o) =>
                  {
                      if (canExecute != null)
                          return canExecute();
                      else
                          return true;
                  })
        {
        }
    }
}
