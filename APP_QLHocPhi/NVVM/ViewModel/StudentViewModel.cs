using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using Microsoft.Win32; // Dùng cho hộp thoại lưu file
using System;
using System.Collections.ObjectModel;
using System.IO; // Dùng để xuất file
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    // Class hiển thị cho DataGrid (Chỉ chứa thông tin môn học)
    public class RegistrationItem : BaseViewModel
    {
        private bool _IsSelected;
        public bool IsSelected { get => _IsSelected; set { _IsSelected = value; OnPropertyChanged(); } }

        public int STT { get; set; }
        public Subject SubjectInfo { get; set; } // Giữ thông tin môn học

        public string SubjectName { get => SubjectInfo?.DisplayName; }
        public int TinChi { get => SubjectInfo?.SoTinChi ?? 0; }
        public string GhiChu { get; set; } // Ghi chú nếu cần
    }

    public class StudentViewModel : BaseViewModel
    {
        // Danh sách môn ĐANG CHỌN (Chưa lưu vào DB)
        private ObservableCollection<RegistrationItem> _ListDangKy;
        public ObservableCollection<RegistrationItem> ListDangKy { get => _ListDangKy; set { _ListDangKy = value; OnPropertyChanged(); } }

        // --- Data Sources ---
        public ObservableCollection<Student> StudentList { get; set; }
        public ObservableCollection<Subject> SubjectList { get; set; }
        public ObservableCollection<TutitionConfig> SemesterList { get; set; }

        // --- Selected Items ---
        private Student _SelectedStudent;
        public Student SelectedStudent { get => _SelectedStudent; set { _SelectedStudent = value; OnPropertyChanged(); } }

        private Subject _SelectedSubject;
        public Subject SelectedSubject { get => _SelectedSubject; set { _SelectedSubject = value; OnPropertyChanged(); } }

        private TutitionConfig _SelectedSemester;
        public TutitionConfig SelectedSemester { get => _SelectedSemester; set { _SelectedSemester = value; OnPropertyChanged(); } }

        // --- Commands ---
        public ICommand AddTempCommand { get; set; } // Thêm vào list tạm trên UI
        public ICommand ConfirmCommand { get; set; } // Lưu vào DB (Tính tiền ngầm)
        public ICommand DeleteTempCommand { get; set; } // Xóa khỏi list tạm
        public ICommand ExportCommand { get; set; } // Xuất danh sách

        public StudentViewModel()
        {
            LoadData();
            ListDangKy = new ObservableCollection<RegistrationItem>();

            // 1. THÊM VÀO DANH SÁCH TẠM (Chưa tính tiền, chỉ hiện UI)
            AddTempCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện để bấm nút: Phải chọn đủ 3 thứ
                return SelectedSubject != null && SelectedStudent != null && SelectedSemester != null;
            }, (p) =>
            {
                // 1. KIỂM TRA TRÙNG TRONG DANH SÁCH TẠM (Logic cũ)
                // (Tránh trường hợp vừa thêm vào list tạm xong lại bấm thêm lần nữa)
                if (ListDangKy.Any(x => x.SubjectInfo.Id == SelectedSubject.Id))
                {
                    MessageBox.Show("Môn này đã có trong danh sách chờ bên dưới rồi!", "Trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. KIỂM TRA TRÙNG TRONG DATABASE (Logic MỚI 🌟)
                // (Tránh trường hợp ông Nhạc đã đăng ký môn này từ trước rồi)
                var db = DataProvider.Ins.DB;
                var daDangKy = db.StudentRegistrations.Any(x =>
                    x.StudentId == SelectedStudent.Id &&
                    x.SubjectId == SelectedSubject.Id &&
                    x.HocKy == SelectedSemester.HocKy); // Kiểm tra cả học kỳ nữa nhé

                if (daDangKy)
                {
                    MessageBox.Show($"Sinh viên {SelectedStudent.DisplayName} ĐÃ ĐĂNG KÝ môn {SelectedSubject.DisplayName} trong học kỳ này rồi!",
                                    "Đã đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Dừng lại, không cho thêm
                }

                // 3. NẾU VƯỢT QUA HẾT CÁC ẢI THÌ MỚI THÊM VÀO LIST
                var item = new RegistrationItem
                {
                    STT = ListDangKy.Count + 1,
                    SubjectInfo = SelectedSubject,
                    IsSelected = true,
                    GhiChu = "Đang chờ xác nhận"
                };
                ListDangKy.Add(item);
            });

            // 2. XÓA KHỎI DANH SÁCH TẠM
            DeleteTempCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                // Xóa các item đang được tick chọn
                var itemsToRemove = ListDangKy.Where(x => x.IsSelected).ToList();
                foreach (var item in itemsToRemove)
                {
                    ListDangKy.Remove(item);
                }
                // Đánh lại số thứ tự
                for (int i = 0; i < ListDangKy.Count; i++) ListDangKy[i].STT = i + 1;
            });

            // 3. XÁC NHẬN ĐĂNG KÝ (Lưu DB + Tính tiền ngầm)
            ConfirmCommand = new RelayCommand<object>((p) =>
            {
               return SelectedStudent != null &&
               SelectedSemester != null &&
               ListDangKy.Any(x => x.IsSelected);
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                decimal donGiaMacDinh = 500000; // Quy định cứng: 500k/1 tín

                var itemsToSave = ListDangKy.Where(x => x.IsSelected).ToList();

                foreach (var item in itemsToSave)
                {
                    bool exists = db.StudentRegistrations.Any(x =>
                        x.StudentId == SelectedStudent.Id &&
                        x.SubjectId == item.SubjectInfo.Id &&
                        x.HocKy == SelectedSemester.HocKy);

                    if (!exists)
                    {
                        decimal donGiaMonHoc = item.SubjectInfo.DonGia ?? donGiaMacDinh;

                        var reg = new StudentRegistration()
                        {
                            StudentId = SelectedStudent.Id,
                            SubjectId = item.SubjectInfo.Id,
                            HocKy = SelectedSemester.HocKy,
                            DonGiaTaiThoiDiemDangKi = donGiaMonHoc,
                            TongTienHoc = item.SubjectInfo.SoTinChi * donGiaMonHoc,
                            SoTienDaDong = 0,
                            TrangThai = "Chưa đóng"
                        };
                        db.StudentRegistrations.Add(reg);
                    }
                }
                db.SaveChanges();
                MessageBox.Show("Đã xác nhận đăng ký thành công vào hệ thống!");
                //Rung chuông refresh data
                DataProvider.Ins.RefreshDataBase();
                foreach (var item in itemsToSave)
                {
                    ListDangKy.Remove(item);
                }
                DataProvider.Ins.RefreshDataBase();
            });

            // 4. XUẤT DANH SÁCH (Export CSV đơn giản)
            ExportCommand = new RelayCommand<object>((p) => ListDangKy.Count > 0, (p) =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV file (*.csv)|*.csv";

                if (saveFileDialog.ShowDialog() == true)
                {
                    // 👉 SỬA DÒNG NÀY: Thêm 'false' (không append) và 'Encoding.UTF8'
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Ghi Header
                        sw.WriteLine("STT,Mã Môn,Tên Môn,Số Tín Chỉ");

                        // Ghi Data
                        foreach (var item in ListDangKy)
                        {
                            // Lưu ý: Nếu tên môn học có dấu phẩy (,), file CSV sẽ bị lệch cột.
                            // Để an toàn hơn, mình nên bọc các trường text trong dấu ngoặc kép ""
                            string line = $"{item.STT},{item.SubjectInfo.Id},\"{item.SubjectName}\",{item.TinChi},";
                            sw.WriteLine(line);
                        }
                    }
                    MessageBox.Show("Xuất file thành công!");
                }
            });
        }

        void LoadData()
        {
            var db = DataProvider.Ins.DB;
            StudentList = new ObservableCollection<Student>(db.Students.ToList());
            SubjectList = new ObservableCollection<Subject>(db.Subjects.ToList());
            SemesterList = new ObservableCollection<TutitionConfig>(db.TutitionConfigs.ToList());
        }
    }
}