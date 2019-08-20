using ScriptLinker.Services;
using ScriptLinker.ViewModels;
using System.Windows;

namespace ScriptLinker.Views
{
    /// <summary>
    /// Interaction logic for CreateNewScriptWindow.xaml
    /// </summary>
    public partial class CreateNewScriptWindow : Window
    {
        private ViewModelBase viewModel;

        public CreateNewScriptWindow()
        {
            InitializeComponent();

            viewModel = new CreateNewScriptViewModel(ApplicationService.Instance.EventAggregator, Close);
            DataContext = viewModel;
        }
    }
}
