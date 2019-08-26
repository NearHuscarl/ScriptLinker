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
        public ScriptInfoForm()
        {
            InitializeComponent();
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
