using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class UserViewModel : BaseViewModel
    {
        private ObservableCollection<User> _List;
        public ObservableCollection<User> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private User _SelectedItem;
        public User SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                    Role = SelectedItem.Role;
                    UserName = SelectedItem.Id; // ID thường không cho sửa, chỉ hiển thị
                }
            }
        }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }

        private string _Role;
        public string Role { get => _Role; set { _Role = value; OnPropertyChanged(); } }

        // Danh sách quyền để đổ vào ComboBox
        public ObservableCollection<string> RoleList { get; set; } = new ObservableCollection<string> { "1", "0" };
        // Quy ước: "1" là Admin, "0" là Nhân viên/Học sinh (tùy nghiệp vụ của bạn)

        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public UserViewModel()
        {
            LoadData();

            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null) return false;
                return true;
            }, (p) =>
            {
                var user = DataProvider.Ins.DB.Users.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                if (user != null)
                {
                    user.DisplayName = DisplayName;
                    user.Role = Role;

                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    LoadData();
                }
            });

            DeleteCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null) return false;
                // Không cho phép xóa chính tài khoản đang đăng nhập
                if (SelectedItem.Id == UserSession.CurrentUser.Id) return false;
                return true;
            }, (p) =>
            {
                var user = DataProvider.Ins.DB.Users.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                if (user != null)
                {
                    if (MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản {user.DisplayName}?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        DataProvider.Ins.DB.Users.Remove(user);
                        DataProvider.Ins.DB.SaveChanges();
                        LoadData();
                    }
                }
            });
        }

        void LoadData()
        {
            List = new ObservableCollection<User>(DataProvider.Ins.DB.Users);
        }
    }
}