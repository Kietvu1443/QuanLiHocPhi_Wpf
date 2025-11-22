using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;

namespace APP_QLHocPhi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Thiết lập đường dẫn DataDirectory về nơi file .exe đang chạy
            string executable = Assembly.GetExecutingAssembly().Location;
            string path = Path.GetDirectoryName(executable);
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }
    }

}
