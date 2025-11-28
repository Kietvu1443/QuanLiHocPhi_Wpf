using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class SubjectViewModel : BaseViewModel
    {
        private ObservableCollection<Subject> _List;
        public ObservableCollection<Subject> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private Subject _SelectedItem;
        public Subject SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    Id = SelectedItem.Id;
                    DisplayName = SelectedItem.DisplayName;
                    SoTinChi = SelectedItem.SoTinChi;
                    DonGia = SelectedItem.DonGia;
                }
            }
        }

        private string _Id;
        public string Id { get => _Id; set { _Id = value; OnPropertyChanged(); } }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private int _SoTinChi;
        public int SoTinChi { get => _SoTinChi; set { _SoTinChi = value; OnPropertyChanged(); } }

        private decimal? _DonGia;
        public decimal? DonGia { get => _DonGia; set { _DonGia = value; OnPropertyChanged(); } }

        private string _NewSemesterName;// Thêm học kì
        public string NewSemesterName { get => _NewSemesterName; set { _NewSemesterName = value; OnPropertyChanged(); } }



        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ICommand AddSemesterCommand { get; set; }

        public SubjectViewModel()
        {
            LoadList();

            // --- THÊM MÔN HỌC ---
            AddCommand = new RelayCommand<object>((p) =>
            {
                if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(DisplayName)) return false;
                var db = DataProvider.Ins.DB;
                if (db.Subjects.Any(x => x.Id == Id)) return false; // Không được trùng mã
                return true;
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var subject = new Subject() { Id = Id, DisplayName = DisplayName, SoTinChi = SoTinChi, DonGia = DonGia};
                db.Subjects.Add(subject);
                db.SaveChanges();
                LoadList();
                MessageBox.Show("Thêm môn học thành công!");
            });

            // --- SỬA MÔN HỌC ---
            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null) return false;
                var db = DataProvider.Ins.DB;
                return db.Subjects.Any(x => x.Id == SelectedItem.Id);
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var subject = db.Subjects.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                subject.DisplayName = DisplayName;
                subject.SoTinChi = SoTinChi;
                subject.DonGia = DonGia;
                db.SaveChanges();
                LoadList();
                MessageBox.Show("Cập nhật thành công!");
            });

            // --- XÓA MÔN HỌC ---
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var subject = db.Subjects.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();

                // Kiểm tra ràng buộc: Nếu môn này đã có sinh viên đăng ký thì không cho xóa
                if (db.StudentRegistrations.Any(x => x.SubjectId == SelectedItem.Id))
                {
                    MessageBox.Show("Không thể xóa môn đã có sinh viên đăng ký!");
                    return;
                }

                db.Subjects.Remove(subject);
                db.SaveChanges();
                LoadList();
            });
            // Thêm học kì
            AddSemesterCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện: Không được để trống
                return !string.IsNullOrEmpty(NewSemesterName);
            }, (p) =>
            {
                // Kiểm tra xem học kỳ này đã có trong Database chưa?
                var exists = DataProvider.Ins.DB.TutitionConfigs.Any(x => x.HocKy == NewSemesterName);
                if (exists)
                {
                    MessageBox.Show("Tên học kỳ này đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Tạo mới object TutitionConfig (Bảng này lưu tên học kỳ)
                var newHK = new TutitionConfig()
                {
                    HocKy = NewSemesterName,
                    // Các giá trị mặc định khác nếu cần (ví dụ: DotThu = 1)
                };

                DataProvider.Ins.DB.TutitionConfigs.Add(newHK);
                DataProvider.Ins.DB.SaveChanges();

                MessageBox.Show($"Đã thêm học kỳ {NewSemesterName} thành công!");

                // Reset ô nhập về trống
                NewSemesterName = "";
            });
        }

        void LoadList()
        {
            List = new ObservableCollection<Subject>(DataProvider.Ins.DB.Subjects.ToList());
        }
    }
}