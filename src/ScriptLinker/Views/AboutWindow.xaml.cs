using ScriptLinker.ViewModels;
using System.Windows;

namespace ScriptLinker.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private ViewModelBase viewModel;

        public AboutWindow()
        {
            InitializeComponent();

            viewModel = new AboutViewModel(Close);
            DataContext = viewModel;
        }
    }
}
