using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    class LoginViewModel : BaseViewModel
    {
        public bool IsLogin { get; set; } // trạng thái đăng nhập
        public User CurrentUser { get; set; } //Kiểm tra user hiện tại

        private string _Id; // Lưu Trữ id đăng nhập
        public string Id { get=> _Id; set { _Id = value;OnPropertyChanged(); } }

        private string _Password; // lưu lại pas khi đăng nhập
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; set; } // lệnh lưu mật khẩu

        public ICommand CloseCommand { get; set; } // lệnh đóng cửa sổ

        public ICommand PasswordChangedCommand { get; set; } // lệnh thay đổi mật khẩu

        public ICommand GuestLogin { get; set; } // lệnh đăng nhập với tư cách khách( sẽ thêm vào sau)

        public ICommand OpenSignUpCommand { get; set; } // lệnh mở cửa sổ đăng kí tài khoản



        //tất cả sẽ được sử lí trong đây    
        public LoginViewModel()
        {

            IsLogin = false;
            LoginCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Login(p); });
            GuestLogin = new RelayCommand<Window>((p) => { return true; }, (p) => { LoginG(p); });
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                if (p == null) return;

                IsLogin = false; // Đặt rõ ràng là false khi bấm thoát
                p.Close();
            });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => { Password = p.Password; });
            OpenSignUpCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                SignUpWindow signUp = new SignUpWindow();
                // Gán DataContext thủ công nếu chưa khai báo Resource
                signUp.DataContext = new SignUpViewModel();
                signUp.ShowDialog(); // ShowDialog để bắt buộc xử lý xong mới quay lại Login
            });

        }
        void LoginG(Window p)
        {
            IsLogin = true;
            p.Close();
        }
        void Login(Window p)
        {
            if (p == null)
                return;

            // Kiểm tra đăng nhập và lấy luôn đối tượng User
            var acc = DataProvider.Ins.DB.Users.Where(x => x.Id == Id && x.Password == Password).FirstOrDefault();

            if (acc != null)
            {
                IsLogin = true;
                CurrentUser = acc; // lưu lại thông tin user để qua main

                UserSession.CurrentUser = acc; // Lưu vào biến toàn cục
                SystemLog.Log("LOGIN", "Đăng nhập vào hệ thống"); // Ghi log 

                p.Close();


            }
            else
            {
                IsLogin = false;
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu vui lòng thử lại");  // báo lỗi
            }
        }
    } 
}
