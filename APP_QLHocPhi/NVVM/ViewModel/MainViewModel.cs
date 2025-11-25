using APP_QLHocPhi.Models;
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

        public ICommand AddStudentCommand { get; set; }

        public ICommand DashBoardCommand { get; set; }

        public ICommand SchoolFeeCommand { get; set; }

        public ICommand StudentCommand { get; set; }

        public ICommand SubjectCommand { get; set; }

        public ICommand UserCommand { get; set; }

        //tất cả sẽ được sử lí trong đây    
        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                Isloaded = true;
                if(p== null)
                {
                    return;
                }
                p.Hide();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.ShowDialog();
                p.Show();

                var LoginVM = loginWindow.DataContext as LoginViewModel;

                if(loginWindow.DataContext == null)
                {
                    return;
                }
                if (LoginVM.IsLogin)
                {
                    p.Show();
                }
                else
                {
                    p.Close();
                }
             
            });

            AddStudentCommand = new RelayCommand<object>((p) => { return true; }, (p) => { AddStudentWindow wd = new AddStudentWindow(); wd.ShowDialog(); });

            DashBoardCommand = new RelayCommand<object>((p) => { return true; }, (p) => { DashBoardWindow wd = new DashBoardWindow(); wd.ShowDialog(); });

            SchoolFeeCommand = new RelayCommand<object>((p) => { return true; }, (p) => { SchoolFeeWindow wd = new SchoolFeeWindow(); wd.ShowDialog(); });

            StudentCommand = new RelayCommand<object>((p) => { return true; }, (p) => { StudentWindow wd = new StudentWindow(); wd.ShowDialog(); });

            UserCommand = new RelayCommand<object>((p) => { return true; }, (p) => { UserWindow wd = new UserWindow(); wd.ShowDialog(); });

            SubjectCommand = new RelayCommand<object>((p) => { return true; }, (p) => { SubjectWindow wd = new SubjectWindow(); wd.ShowDialog(); });
            //MessageBox.Show(DataProvider.Ins.DB.Users.First().Role);
        }
    }
}
