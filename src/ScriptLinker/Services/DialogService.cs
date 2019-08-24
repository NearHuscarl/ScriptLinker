using System;
using System.Windows;
using System.Windows.Threading;

namespace ScriptLinker.Services
{
    class DialogService
    {
        public void ShowConfirmDialog(string message, Action yesCallback, Action noCallback = null)
        {
            var result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                yesCallback.Invoke();
            if (result == MessageBoxResult.No)
                noCallback?.Invoke();
        }

        public void ShowInfoDialog(string message)
        {
            MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarningDialog(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
