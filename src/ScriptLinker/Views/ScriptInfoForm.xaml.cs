using ScriptLinker.Models;
using ScriptLinker.Services;
using ScriptLinker.Utilities;
using ScriptLinker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptLinker.Views
{
    /// <summary>
    /// Interaction logic for ScriptInfoForm.xaml
    /// </summary>
    public partial class ScriptInfoForm : UserControl
    {
        private IValidator viewModel;

        public ScriptInfoForm()
        {
            InitializeComponent();

            viewModel = new ScriptInfoFormViewModel(ApplicationService.Instance.EventAggregator);
            DataContext = viewModel;
        }

        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
            "ButtonContent",
            typeof(string),
            typeof(ScriptInfoForm),
            new PropertyMetadata("Create"));

        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public static readonly DependencyProperty OnSubmitProperty = DependencyProperty.Register(
            "OnSubmit",
            typeof(Action<ScriptInfo>),
            typeof(ScriptInfoForm),
            new PropertyMetadata(null, OnSubmitPropertyChanged));

        private static void OnSubmitPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            var scriptInfoForm = (ScriptInfoForm)dpo;
            var vm = (IValidator)scriptInfoForm.DataContext;
            var onSubmit = (Action<ScriptInfo>)e.NewValue;
            var command = new DelegateCommand<ScriptInfo>((scriptInfo) =>
            {
                if (vm.Validate())
                    onSubmit(scriptInfo);
            }, null); // set canExecute callback to null because it will be run once on startup which will set error messages

            scriptInfoForm.SubmitCommand = command;
        }

        public Action<ScriptInfo> OnSubmit
        {
            get { return (Action<ScriptInfo>)GetValue(OnSubmitProperty); }
            set { SetValue(OnSubmitProperty, value); }
        }

        public static readonly DependencyProperty SubmitCommandProperty = DependencyProperty.Register(
           "SubmitCommand",
           typeof(ICommand),
           typeof(ScriptInfoForm),
           new PropertyMetadata(null));

        public ICommand SubmitCommand
        {
            get { return (ICommand)GetValue(SubmitCommandProperty); }
            private set { SetValue(SubmitCommandProperty, value); }
        }

        public static readonly DependencyProperty DisplaySubmitButtonProperty = DependencyProperty.Register(
            "DisplaySubmitButton",
            typeof(bool),
            typeof(ScriptInfoForm),
            new PropertyMetadata(true));

        public bool DisplaySubmitButton
        {
            get { return (bool)GetValue(DisplaySubmitButtonProperty); }
            set { SetValue(DisplaySubmitButtonProperty, value); }
        }

        public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register(
            "HotKey",
            typeof(Key),
            typeof(ScriptInfoForm),
            new PropertyMetadata(null));

        public Key HotKey
        {
            get { return (Key)GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }

        public static readonly DependencyProperty HotKeyModifiersProperty = DependencyProperty.Register(
            "HotKeyModifiers",
            typeof(ModifierKeys),
            typeof(ScriptInfoForm),
            new PropertyMetadata(null));

        public ModifierKeys HotKeyModifiers
        {
            get { return (ModifierKeys)GetValue(HotKeyModifiersProperty); }
            set { SetValue(HotKeyModifiersProperty, value); }
        }
    }
}
