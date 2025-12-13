using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    
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
        
        private ObservableCollection<StudentDisplayItem> _List;
        public ObservableCollection<StudentDisplayItem> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        
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

        //Các Property Binding
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

        private string _SearchKeyword;
        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                _SearchKeyword = value;
                OnPropertyChanged();
                FilterList(); // QUAN TRỌNG: Gõ phím là Lọc 
            }
        }


        // Command
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        public AddStudentViewModel()
        {
            LoadList();
            //  Đăng ký nhận thông báo khi Database thay đổi
            // (Bất kỳ ai gọi RefreshDataBase thì hàm LoadList của tui sẽ tự chạy)
            DataProvider.Ins.DatabaseChanged += LoadList;

            // Lệnh thêm
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
                   TrangThai = string.IsNullOrEmpty(TrangThai) ? "Đang học" : TrangThai
                };

                if (NgaySinh.HasValue)
                    student.NgaySinh = DateOnly.FromDateTime(NgaySinh.Value);

                SplitName(DisplayName, out string hoDem, out string ten);
                student.HoDem = hoDem;
                student.Ten = ten;

                DataProvider.Ins.DB.Students.Add(student);
                DataProvider.Ins.DB.SaveChanges();

                SystemLog.Log("THÊM SINH VIÊN", $"Đã thêm SV: {student.DisplayName} - MSSV: {student.Id}");

                DataProvider.Ins.RefreshDataBase(); // Thông báo thay đổi Database




                ClearInputs();
            });

            // --- LỆNH SỬA ---
            EditCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện để bấm nút Sửa: Phải đang chọn một sinh viên trong danh sách
                return SelectedItem != null;
            }, (p) =>
            {
                // Lấy ID của sinh viên đang chọn ( id cố định)
                var idCanSua = SelectedItem.StudentInfo.Id;

                // Tìm sinh viên đó trong Database
                var student = DataProvider.Ins.DB.Students.SingleOrDefault(x => x.Id == idCanSua);

                if (student != null)
                {
                    // ẬP NHẬT THÔNG TIN (Trừ ID)
                    student.DisplayName = DisplayName;
                    student.Lop = ClassName;  
                    student.Nganh = MajorName;
                    student.Phone = Phone;
                    student.Address = Address;
                    student.Email = Email;
                    student.GioiTinh = GioiTinh;
                    student.TrangThai = TrangThai;

                    // Xử lý Ngày sinh
                    if (NgaySinh.HasValue)
                        student.NgaySinh = DateOnly.FromDateTime(NgaySinh.Value);
                    else
                        student.NgaySinh = null;

                    // Tách Họ Tên
                    SplitName(DisplayName, out string hoDem, out string ten);
                    student.HoDem = hoDem;
                    student.Ten = ten;

                    // Lưu vào Database
                    DataProvider.Ins.DB.SaveChanges();

                    // Rung chuông báo hiệu cho các màn hình khác (Dashboard, Thu phí...) biết
                    DataProvider.Ins.RefreshDataBase();

                    // Tải lại danh sách tại chỗ và xóa trắng ô nhập
                   
                    ClearInputs();

                    MessageBox.Show("Cập nhật thông tin thành công!");
                }
            });

            // --- LỆNH XÓA ---
            RemoveCommand = new RelayCommand<object>((p) =>
            {
                return SelectedItem != null;
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var student = db.Students.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();

                if (student == null) return;

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sinh viên {student.DisplayName}?\n\nTất cả dữ liệu điểm số, học phí, hóa đơn liên quan sẽ bị xóa vĩnh viễn!",
                                             "Cảnh báo xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // XÓA HÓA ĐƠN & CHI TIẾT HÓA ĐƠN ---
                        // Tìm tất cả hóa đơn của sinh viên này
                        var invoices = db.Invoices.Where(x => x.StudentId == student.Id).ToList();

                        if (invoices.Count > 0)
                        {
                            // Lấy danh sách ID hóa đơn
                            var invoiceIds = invoices.Select(x => x.Id).ToList();

                            // Xóa Chi tiết hóa đơn trước (Con của Hóa đơn)
                            var details = db.InvoiceDetails.Where(x => invoiceIds.Contains(x.InvoiceId)).ToList();
                            db.InvoiceDetails.RemoveRange(details);

                            // Sau đó mới xóa Hóa đơn
                            db.Invoices.RemoveRange(invoices);
                        }

                        //XÓA ĐĂNG KÝ MÔN HỌC ---
                        var registrations = db.StudentRegistrations.Where(x => x.StudentId == student.Id).ToList();
                        db.StudentRegistrations.RemoveRange(registrations);

                        //XÓA SINH VIÊN
                        db.Students.Remove(student);

                        // Lưu tất cả thay đổi
                        db.SaveChanges();
                        //Ghi log
                        SystemLog.Log("XOÁ SINH VIÊN", $"Đã xoá sinh viên: {student.DisplayName} - MSSV: {student.Id}");


                        // Cập nhật lại giao diện
                        DataProvider.Ins.RefreshDataBase();
                        MessageBox.Show("Đã xóa sinh viên và toàn bộ dữ liệu liên quan thành công!");

                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Vẫn không xóa được. Lỗi chi tiết: " + ex.Message);
                    }
                }
            });
        }

        void LoadList()
        {
            var listFromDB = DataProvider.Ins.DB.Students.ToList();
            //Khởi tạo danh sách với kiểu mới StudentDisplayItem
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

            //Nếu đang tìm kiếm thì lọc lại ngay lập tức
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                FilterList();
            }
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
        //Hàm xử lý lọc danh sách
        private void FilterList()
        {
            // Lấy View mặc định của danh sách List
            ICollectionView view = CollectionViewSource.GetDefaultView(List);

            if (view == null) return; // Kiểm tra an toàn

            if (string.IsNullOrEmpty(SearchKeyword))
            {
                view.Filter = null; // Nếu không gõ gì thì hiện hết
            }
            else
            {
                view.Filter = (obj) =>
                {
                    // Ép kiểu object về StudentDisplayItem để kiểm tra
                    var item = obj as StudentDisplayItem;
                    if (item == null) return false;

                    // Chuẩn hóa chữ thường để tìm không phân biệt hoa thường
                    string keyword = SearchKeyword.ToLower();

                    // Lấy dữ liệu để so sánh (Dùng ?. để tránh lỗi null)
                    string ten = item.StudentInfo.Ten?.ToLower() ?? "";
                    string hoDem = item.StudentInfo.HoDem?.ToLower() ?? "";
                    string maSV = item.Id?.ToLower() ?? "";
                    string hoTenDayDu = item.DisplayName?.ToLower() ?? "";

                    // Logic: Tìm thấy trong Tên HOẶC Mã SV HOẶC Họ Đệm HOẶC Tên đầy đủ
                    return ten.Contains(keyword)
                        || maSV.Contains(keyword)
                        || hoDem.Contains(keyword)
                        || hoTenDayDu.Contains(keyword);
                };
            }
        }
    }
}