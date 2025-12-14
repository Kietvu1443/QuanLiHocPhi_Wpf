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
        public bool Isloaded = false; // đã được load chưa ?
        public User CurrentUser { get; private set; }  // Biến này để lưu user hiện tại

        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName;
            set { _DisplayName = value; OnPropertyChanged(); }
        }

        // PHẦN PHÂN QUYỀN

        // Admin thấy: Dashboard, Thêm HS, Thu học phí, Quản lý môn, tất cả nút quản lý
        private Visibility _AdminVisibility = Visibility.Collapsed; // Mặc định là ẩn
        public Visibility AdminVisibility
        {
            get => _AdminVisibility;
            set { _AdminVisibility = value; OnPropertyChanged(); }
        }
        // Học sinh thấy: Nút Student (nếu cần) và Nút In hóa đơn
        private Visibility _StudentVisibility = Visibility.Collapsed;
        public Visibility StudentVisibility
        {
            get => _StudentVisibility;
            set { _StudentVisibility = value; OnPropertyChanged(); }
        }

        public ICommand LoadedWindowCommand { get; set; }

        public ICommand AddStudentCommand { get; set; }

        public ICommand DashBoardCommand { get; set; }

        public ICommand SchoolFeeCommand { get; set; }

        public ICommand StudentCommand { get; set; }

        public ICommand SubjectCommand { get; set; }

        public ICommand UserCommand { get; set; }

        public ICommand LogoutCommand { get; set; }
        public ICommand StudentInvoiceCommand { get; set; }  // Command mới cho học sinh xem hóa đơn

       

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
                    SetUser(LoginVM.CurrentUser);
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

            

            SubjectCommand = new RelayCommand<object>((p) => { return true; }, (p) => { SubjectWindow wd = new SubjectWindow(); wd.ShowDialog(); });

            UserCommand = new RelayCommand<object>((p) => { return true; }, (p) => {
                UserWindow wd = new UserWindow();
                wd.ShowDialog();
            });

            StudentInvoiceCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                // Gọi cửa sổ mới tạo
                StudentInvoiceWindow wd = new StudentInvoiceWindow(CurrentUser);
                wd.ShowDialog();
            });
            //MessageBox.Show(DataProvider.Ins.DB.Users.First().Role);
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                AdminVisibility = Visibility.Visible;
                StudentVisibility = Visibility.Visible;
            }



            LogoutCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                if (p == null) return;

                // 1. Đóng các cửa sổ con và dọn dữ liệu user cũ
                foreach (Window window in Application.Current.Windows.OfType<Window>().ToList())
                {
                    if (window != p) window.Close();
                }
                CurrentUser = null;

                // Ẩn MainWindow
                p.Hide();

                // Tạo LoginWindow mới
                LoginWindow loginWindow = new LoginWindow();

           
                // Dòng này đảm bảo IsLogin = false, xóa sạch trạng thái đăng nhập cũ
                loginWindow.DataContext = new LoginViewModel();
             

                loginWindow.ShowDialog();

                //Kiểm tra kết quả từ ViewModel MỚI này
                var loginVM = loginWindow.DataContext as LoginViewModel;

                if (loginVM != null && loginVM.IsLogin)
                {
                    // Đăng nhập lại thành công
                    SetUser(loginVM.CurrentUser);
                    p.Show();
                }
                else
                {
                    // Vì là ViewModel mới nên IsLogin mặc định là false -> Vào đây -> Đóng App
                    p.Close();
                }
            });
        }
        public void SetUser(User user)
        {
            CurrentUser = user;
            if (CurrentUser == null) return;

            DisplayName = CurrentUser.DisplayName;

            if (CurrentUser.Role == "1") // 1 Là Admin
            {
                AdminVisibility = Visibility.Visible; // Hiện hết chức năng quản lý
                StudentVisibility = Visibility.Visible; // Hiện cả chức năng của user
            }
            else // 0 Là User (Học sinh)
            {
                AdminVisibility = Visibility.Collapsed; // Ẩn hết chức năng quản lý
                StudentVisibility = Visibility.Visible; // Chỉ hiện chức năng cho user
            }
        }

    }
}
