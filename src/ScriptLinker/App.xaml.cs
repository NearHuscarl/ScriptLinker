using System.Windows;

namespace ScriptLinker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Current.Properties[Constants.SourceCodeUrl] = "https://github.com/NearHuscarl/ScriptLinker";
        }
    }
}
