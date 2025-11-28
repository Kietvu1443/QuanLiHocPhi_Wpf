using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class SignUpViewModel : BaseViewModel
    {
        private string _Id;
        public string Id { get => _Id; set { _Id = value; OnPropertyChanged(); } }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _SelectedRole;
        public string SelectedRole { get => _SelectedRole; set { _SelectedRole = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Roles { get; set; }

        public ICommand SignUpCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public SignUpViewModel()
        {
            // Hiển thị tên tiếng Việt cho dễ hiểu
            Roles = new ObservableCollection<string>() { "Quản trị viên (Admin)", "Nhân viên (Staff)" };

            SignUpCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { Register(p); });
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p.Close(); });
        }

        void Register(Window p)
        {
            if (p == null) return;

            // Kiểm tra nhập thiếu
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(SelectedRole))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo");
                return;
            }

            var passwordBox = p.FindName("FloatingPasswordBox") as PasswordBox;
            string password = passwordBox?.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo");
                return;
            }

            // Kiểm tra trùng ID
            var exist = DataProvider.Ins.DB.Users.Any(x => x.Id == Id);
            if (exist)
            {
                MessageBox.Show("Tên đăng nhập này đã tồn tại!", "Lỗi");
                return;
            }

            // XỬ LÝ QUYỀN HẠN (QUAN TRỌNG)
            // MainViewModel check: "1" là Admin, còn lại là User
            string roleToSave = "0"; // Mặc định là 0 (Nhân viên)

            if (SelectedRole.Contains("Quản trị viên"))
            {
                roleToSave = "1";
            }
            // Ngược lại vẫn là "0"

            // 4. Lưu vào Database
            var newUser = new User()
            {
                Id = Id,
                DisplayName = DisplayName,
                Password = password,
                Role = roleToSave // Lưu "1" hoặc "0"
            };

            DataProvider.Ins.DB.Users.Add(newUser);
            DataProvider.Ins.DB.SaveChanges();

            MessageBox.Show($"Đăng ký thành công tài khoản: {DisplayName}\nQuyền: {(roleToSave == "1" ? "Admin" : "Nhân viên")}");

            p.Close();
        }
    }
}