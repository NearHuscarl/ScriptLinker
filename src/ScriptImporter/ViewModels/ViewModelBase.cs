using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace ScriptImporter.ViewModels
{
    /// <summary>
    /// https://stackoverflow.com/a/36151255/9449426
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
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
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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

            this.NotifyPropertyChanged(propertyName);

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

        public virtual void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // override me
        }
    }
}
