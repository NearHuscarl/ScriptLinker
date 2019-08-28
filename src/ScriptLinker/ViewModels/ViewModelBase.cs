using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace ScriptLinker.ViewModels
{
    /// <summary>
    /// https://stackoverflow.com/a/36151255/9449426
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify the view that the property propertyName value has changed
        /// 
        /// the [CallerMemberName] attribute is not required, but it will allow to
        /// write: OnPropertyChanged(); instead of OnPropertyChanged("SomeProperty");
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null,
            object before = null, object after = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropsChanged(propertyName, before, after);
        }

        // Don't use OnPropertyChanged. It's a reserved method name that will be automatically called
        // by Fody
        protected virtual void OnPropsChanged(string propertyName, object before, object after)
        {
        }

        #endregion

        #region Setter Wrappers

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;

            return true;
        }
        protected virtual bool SetPropertyAndNotify<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            var result = SetProperty(ref storage, value);

            NotifyPropertyChanged(propertyName, null, value);

            return result;
        }

        #endregion

        protected void DispatchIfNecessary(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    action.Invoke();
                }));
            }
        }

        #region Validation helpers

        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors[propertyName] = new List<string> { error };
                RaiseErrorsChanged(propertyName);
            }
        }

        protected void RemoveError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        protected void ClearErrors()
        {
            var propertyNames = _errors.Keys.ToList();

            _errors.Clear();
            foreach (var propertyName in propertyNames)
            {
                RaiseErrorsChanged(propertyName);
            }
        }

        protected string GetError(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return "";

            return _errors[propertyName].First();
        }

        #region INotifyDataErrorInfo Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return null;

            return _errors[propertyName];
        }

        public bool HasErrors => _errors.Count > 0;

        #endregion

        #endregion

        public virtual void OnWindowClosing(object sender, CancelEventArgs e)
        {
        }

        /// <summary>
        /// Dispose all managed and unmanaged resources here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnWindowClosed(object sender, EventArgs e)
        {
        }

        public Action Close { get; protected set; }
    }
}
