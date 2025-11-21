using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class AddStudentViewModel : BaseViewModel
    {
        // Danh sách hiển thị lên ListView
        private ObservableCollection<Student> _List;
        public ObservableCollection<Student> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        // Xử lý khi chọn 1 dòng trong danh sách
        private Student _SelectedItem;
        public Student SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    MSSV = SelectedItem.Id;
                    DisplayName = SelectedItem.DisplayName;
                    ClassName = SelectedItem.Lop;
                    MajorName = SelectedItem.Nganh;
                    QrCode = SelectedItem.Phone; // Mình tạm dùng trường Phone làm QRCode/SĐT nhé
                }
            }
        }

        // Các thuộc tính Binding với TextBox
        private string _MSSV;
        public string MSSV { get => _MSSV; set { _MSSV = value; OnPropertyChanged(); } }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _ClassName;
        public string ClassName { get => _ClassName; set { _ClassName = value; OnPropertyChanged(); } }

        private string _MajorName;
        public string MajorName { get => _MajorName; set { _MajorName = value; OnPropertyChanged(); } }

        private string _QrCode;
        public string QrCode { get => _QrCode; set { _QrCode = value; OnPropertyChanged(); } }

        // Các Command (Lệnh)
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        public AddStudentViewModel()
        {
            LoadList();

            // --- LỆNH THÊM ---
            AddCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện: Các ô quan trọng không được trống
                if (string.IsNullOrEmpty(MSSV) || string.IsNullOrEmpty(DisplayName))
                    return false;

                // Kiểm tra trùng MSSV trong Database
                var displayList = DataProvider.Ins.DB.Students.Where(x => x.Id == MSSV);
                if (displayList == null || displayList.Count() != 0)
                    return false;

                return true;

            }, (p) =>
            {
                var student = new Student()
                {
                    Id = MSSV,
                    DisplayName = DisplayName,
                    Lop = ClassName,
                    Nganh = MajorName,
                    Phone = QrCode,
                    TrangThai = "Đang học"
                };

                // Tách họ tên (Vì DB bắt buộc có HoDem và Ten)
                SplitName(DisplayName, out string hoDem, out string ten);
                student.HoDem = hoDem;
                student.Ten = ten;

                DataProvider.Ins.DB.Students.Add(student);
                DataProvider.Ins.DB.SaveChanges();

                List.Add(student); // Cập nhật UI ngay lập tức
                ClearInputs();
            });

            // --- LỆNH SỬA ---
            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null || DataProvider.Ins.DB.Students.Where(x => x.Id == SelectedItem.Id).Count() == 0)
                    return false;
                return true;

            }, (p) =>
            {
                var student = DataProvider.Ins.DB.Students.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                if (student != null)
                {
                    student.DisplayName = DisplayName;
                    student.Lop = ClassName;
                    student.Nganh = MajorName;
                    student.Phone = QrCode;

                    SplitName(DisplayName, out string hoDem, out string ten);
                    student.HoDem = hoDem;
                    student.Ten = ten;

                    DataProvider.Ins.DB.SaveChanges();

                    // Refresh lại list để hiển thị thông tin mới nhất
                    LoadList();
                    ClearInputs(); // Sửa xong thì xóa trắng ô nhập
                }
            });

            // --- LỆNH XÓA ---
            RemoveCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                return true;
            }, (p) =>
            {
                var student = DataProvider.Ins.DB.Students.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();

                try
                {
                    DataProvider.Ins.DB.Students.Remove(student);
                    DataProvider.Ins.DB.SaveChanges();
                    List.Remove(student);
                    ClearInputs();
                }
                catch (Exception)
                {
                    MessageBox.Show("Không thể xóa sinh viên này vì đã có dữ liệu học phí/hóa đơn liên quan!", "Cảnh báo");
                }
            });
        }

        void LoadList()
        {
            List = new ObservableCollection<Student>(DataProvider.Ins.DB.Students);
        }

        void ClearInputs()
        {
            MSSV = ""; DisplayName = ""; ClassName = ""; MajorName = ""; QrCode = "";
        }

        private void SplitName(string fullName, out string hoDem, out string ten)
        {
            fullName = fullName?.Trim() ?? "";
            int lastSpaceIndex = fullName.LastIndexOf(' ');
            if (lastSpaceIndex > 0)
            {
                hoDem = fullName.Substring(0, lastSpaceIndex);
                ten = fullName.Substring(lastSpaceIndex + 1);
            }
            else
            {
                hoDem = "";
                ten = fullName;
            }
        }
    }
}