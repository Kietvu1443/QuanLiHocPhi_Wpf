using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public bool Isloaded = false;

        public ICommand LoadedWindowCommand { get; set; }

        //tất cả sẽ được sử lí trong đây    
        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                //Isloaded = true;
                //LoginWindow loginWindow = new LoginWindow();
                //loginWindow.ShowDialog();

                var main = Application.Current.MainWindow as Window;
                main?.Hide();

                var login = new LoginWindow
                {
                    Owner = main,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Topmost = true
                };

                // Khi login đóng (dù là bấm X hay login thành công)
                login.Closed += (s, e) =>
                {
                    main?.Show();
                    main?.Activate();
                };

                login.Show();
            });           
        }
    }
}
