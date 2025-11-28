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
        public App()
        {

            // Lấy đường dẫn thư mục chứa file .exe đang chạy
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Gán nó vào biến |DataDirectory|
            AppDomain.CurrentDomain.SetData("DataDirectory", executableLocation);

            // --------------------------------------------------------
        }
    }

}
