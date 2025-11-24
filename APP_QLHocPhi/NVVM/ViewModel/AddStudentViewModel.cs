using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    // 1. ĐỔI TÊN CLASS NÀY ĐỂ KHÔNG TRÙNG VỚI DASHBOARD
    public class StudentDisplayItem
    {
        public int STT { get; set; }
        public Student StudentInfo { get; set; }

        // Các thuộc tính cầu nối để Binding
        public string Id { get => StudentInfo.Id; }
        public string DisplayName { get => StudentInfo.DisplayName; }
        public string Lop { get => StudentInfo.Lop; }
        public string Nganh { get => StudentInfo.Nganh; }
        public string Phone { get => StudentInfo.Phone; }
        public string Address { get => StudentInfo.Address; }
        public string Email { get => StudentInfo.Email; }
        public string GioiTinh { get => StudentInfo.GioiTinh; }
        public DateOnly? NgaySinh { get => StudentInfo.NgaySinh; }

        public string TrangThai { get => StudentInfo.TrangThai; }
    }

    public class AddStudentViewModel : BaseViewModel
    {
        // 2. Sửa kiểu dữ liệu List thành StudentDisplayItem
        private ObservableCollection<StudentDisplayItem> _List;
        public ObservableCollection<StudentDisplayItem> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        // 3. Sửa SelectedItem thành kiểu StudentDisplayItem
        private StudentDisplayItem _SelectedItem;
        public StudentDisplayItem SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    // Lấy thông tin từ StudentInfo
                    var s = SelectedItem.StudentInfo;

                    MSSV = s.Id;
                    DisplayName = s.DisplayName;
                    ClassName = s.Lop;
                    MajorName = s.Nganh;
                    Phone = s.Phone;
                    Address = s.Address;
                    Email = s.Email;
                    GioiTinh = s.GioiTinh;
                    TrangThai = s.TrangThai;

                    if (s.NgaySinh.HasValue)
                        NgaySinh = s.NgaySinh.Value.ToDateTime(TimeOnly.MinValue);
                    else
                        NgaySinh = null;
                }
            }
        }

        // --- Các Property Binding (Giữ nguyên) ---
        private string _MSSV;
        public string MSSV { get => _MSSV; set { _MSSV = value; OnPropertyChanged(); } }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _ClassName;
        public string ClassName { get => _ClassName; set { _ClassName = value; OnPropertyChanged(); } }

        private string _MajorName;
        public string MajorName { get => _MajorName; set { _MajorName = value; OnPropertyChanged(); } }

        private string _Phone;
        public string Phone { get => _Phone; set { _Phone = value; OnPropertyChanged(); } }

        private string _Address;
        public string Address { get => _Address; set { _Address = value; OnPropertyChanged(); } }

        private string _Email;
        public string Email { get => _Email; set { _Email = value; OnPropertyChanged(); } }

        private string _GioiTinh;
        public string GioiTinh { get => _GioiTinh; set { _GioiTinh = value; OnPropertyChanged(); } }

        private string _TrangThai;
        public string TrangThai { get => _TrangThai; set { _TrangThai = value; OnPropertyChanged(); } }

        private DateTime? _NgaySinh;
        public DateTime? NgaySinh { get => _NgaySinh; set { _NgaySinh = value; OnPropertyChanged(); } }


        // --- Commands ---
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        public AddStudentViewModel()
        {
            LoadList();

            // --- LỆNH THÊM ---
            AddCommand = new RelayCommand<object>((p) =>
            {
                if (string.IsNullOrEmpty(MSSV) || string.IsNullOrEmpty(DisplayName)) return false;
                var exist = DataProvider.Ins.DB.Students.Any(x => x.Id == MSSV);
                if (exist) return false;
                return true;
            }, (p) =>
            {
                var student = new Student()
                {
                    Id = MSSV,
                    DisplayName = DisplayName,
                    Lop = ClassName,
                    Nganh = MajorName,
                    Phone = Phone,
                    Address = Address,
                    Email = Email,
                    GioiTinh = GioiTinh,
                   TrangThai = string.IsNullOrEmpty(TrangThai) ? "Chưa Đóng" : TrangThai
                };

                if (NgaySinh.HasValue)
                    student.NgaySinh = DateOnly.FromDateTime(NgaySinh.Value);

                SplitName(DisplayName, out string hoDem, out string ten);
                student.HoDem = hoDem;
                student.Ten = ten;

                DataProvider.Ins.DB.Students.Add(student);
                DataProvider.Ins.DB.SaveChanges();

                // Báo hiệu đã thay đổi
                DataProvider.Ins.RefreshDataBase();


                LoadList();
                ClearInputs();
            });

            // --- LỆNH SỬA ---
            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null) return false;
                return true;
            }, (p) =>
            {
                var idCanSua = SelectedItem.StudentInfo.Id;
                var student = DataProvider.Ins.DB.Students.SingleOrDefault(x => x.Id == idCanSua);

                if (student != null)
                {
                    student.DisplayName = DisplayName;
                    student.Lop = ClassName;
                    student.Nganh = MajorName;
                    student.Phone = Phone;
                    student.Address = Address;
                    student.Email = Email;
                    student.GioiTinh = GioiTinh;
                    student.TrangThai = TrangThai;

                    if (NgaySinh.HasValue)
                        student.NgaySinh = DateOnly.FromDateTime(NgaySinh.Value);
                    else
                        student.NgaySinh = null;

                    SplitName(DisplayName, out string hoDem, out string ten);
                    student.HoDem = hoDem;
                    student.Ten = ten;

                    DataProvider.Ins.DB.SaveChanges();

                    //Lệnh báo hiệu đã đổi thay

                    DataProvider.Ins.RefreshDataBase();

                    LoadList();
                    ClearInputs();
                }
            });

            // --- LỆNH XÓA ---
            RemoveCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null) return false;
                return true;
            }, (p) =>
            {
                var idCanXoa = SelectedItem.StudentInfo.Id;
                var student = DataProvider.Ins.DB.Students.SingleOrDefault(x => x.Id == idCanXoa);
                try
                {
                    DataProvider.Ins.DB.Students.Remove(student);
                    DataProvider.Ins.DB.SaveChanges();

                    DataProvider.Ins.RefreshDataBase();

                    LoadList();
                    ClearInputs();
                }
                catch (Exception)
                {
                    MessageBox.Show("Không thể xóa sinh viên này vì đã có dữ liệu liên quan!", "Lỗi");
                }
            });
        }

        void LoadList()
        {
            var listFromDB = DataProvider.Ins.DB.Students.ToList();
            // 4. Khởi tạo danh sách với kiểu mới StudentDisplayItem
            var displayList = new ObservableCollection<StudentDisplayItem>();

            int stt = 1;
            foreach (var item in listFromDB)
            {
                displayList.Add(new StudentDisplayItem
                {
                    STT = stt,
                    StudentInfo = item
                });
                stt++;
            }

            List = displayList;
        }

        void ClearInputs()
        {
            MSSV = ""; DisplayName = ""; ClassName = ""; MajorName = "";
            Phone = ""; Address = ""; Email = ""; GioiTinh = ""; NgaySinh = null; TrangThai = "";
            SelectedItem = null;
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