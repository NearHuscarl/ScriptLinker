using ScriptLinker.Services;
using ScriptLinker.ViewModels;
using System.Windows;

namespace ScriptLinker.Views
{
    /// <summary>
    /// Interaction logic for OptionWindow.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        private ViewModelBase viewModel;

        public OptionWindow()
        {
            InitializeComponent();

            viewModel = new OptionViewModel(ApplicationService.Instance.EventAggregator, Close);
            DataContext = viewModel;
        }
    }
}
