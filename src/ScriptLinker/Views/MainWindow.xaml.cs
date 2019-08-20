using ScriptLinker.Services;
using ScriptLinker.Utilities;
using ScriptLinker.ViewModels;
using System;
using System.Windows;

namespace ScriptLinker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
	    private ViewModelBase viewModel;
        private ScriptInfoForm form;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainViewModel(ApplicationService.Instance.EventAggregator)
            {
                Save = SaveAction,
                OpenNewScriptWindow = OpenNewWindowAction,
            };
            DataContext = viewModel;
            Closing += viewModel.OnWindowClosing;
            Closed += viewModel.OnWindowClosed;
        }

        private Action SaveAction
        {
            get
            {
                return () =>
                {
                    // Focus on ScriptInfoForm usercontrol since the hotkey is binded there, not the main window
                    form.Focus();
                    WinUtil.SimulateKey("^(s)"); // Press Ctrl-S hotkey
                };
            }
        }

        private void ScriptInfoForm_Loaded(object sender, RoutedEventArgs e)
        {
            form = (ScriptInfoForm)sender;
        }

        private void OpenNewWindowAction()
        {
            var window = new CreateNewScriptWindow();

            // Make child window always on top of this window but not all other windows
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
