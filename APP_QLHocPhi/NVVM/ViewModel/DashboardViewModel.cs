using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class DashboardViewModel : BaseViewModel
    {
        //CÁC BIẾN CHO THẺ TỔNG QUAN
        private int _TotalStudent;
        public int TotalStudent { get => _TotalStudent; set { _TotalStudent = value; OnPropertyChanged(); } }

        private decimal _TongDoanhThu;
        public decimal TongDoanhThu { get => _TongDoanhThu; set { _TongDoanhThu = value; OnPropertyChanged(); } }

        private decimal _TongCongNo;
        public decimal TongCongNo { get => _TongCongNo; set { _TongCongNo = value; OnPropertyChanged(); } }

        //CÁC BIẾN CHO BIỂU ĐỒ & LỌC
        private SeriesCollection _SeriesCollection;
        public SeriesCollection SeriesCollection { get => _SeriesCollection; set { _SeriesCollection = value; OnPropertyChanged(); } }

        private string[] _Labels;
        public string[] Labels { get => _Labels; set { _Labels = value; OnPropertyChanged(); } }

        public Func<double, string> Formatter { get; set; }

        // Danh sách học kỳ để hiển thị lên ComboBox
        private ObservableCollection<string> _Semesters;
        public ObservableCollection<string> Semesters { get => _Semesters; set { _Semesters = value; OnPropertyChanged(); } }

        // Học kỳ đang được chọn
        private string _SelectedSemester;
        public string SelectedSemester
        {
            get => _SelectedSemester;
            set
            {
                _SelectedSemester = value;
                OnPropertyChanged();
                LoadChartData(); // Chọn xong là vẽ lại biểu đồ
            }
        }

        public DashboardViewModel()
        {
            LoadGlobalStats(); // Load số liệu tổng
            LoadSemesters();   // Load danh sách học kỳ

            // Định dạng tiền tệ cho biểu đồ
            Formatter = value => value.ToString("N0");

            DataProvider.Ins.DatabaseChanged += () =>
            {
                // Dùng Dispatcher để đảm bảo chạy trên luồng giao diện
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Reload lại toàn bộ thông tin
                    LoadGlobalStats();
                    LoadSemesters(); // Load lại nhỡ có học kỳ mới
                    LoadChartData(); // Vẽ lại biểu đồ
                });
            };
        }

        void LoadGlobalStats()
        {
            var db = DataProvider.Ins.DB;

            // 1. Tổng sinh viên
            TotalStudent = db.Students.Count();

            // 2. Tổng doanh thu (Toàn bộ hóa đơn)
            TongDoanhThu = db.Invoices.Sum(x => x.TongTienThu);

            // 3. Tổng công nợ (Toàn bộ đăng ký)
            TongCongNo = db.StudentRegistrations.Sum(x => x.TongTienHoc - x.SoTienDaDong);
        }

        void LoadSemesters()
        {
            var db = DataProvider.Ins.DB;

            // Lấy danh sách học kỳ duy nhất từ bảng đăng ký môn học
            var listHK = db.StudentRegistrations.Select(x => x.HocKy).Distinct().OrderBy(x => x).ToList();

            Semesters = new ObservableCollection<string>(listHK);

            // Thêm lựa chọn "Tất cả" lên đầu
            Semesters.Insert(0, "Tất cả");

            // Mặc định chọn "Tất cả"
            SelectedSemester = "Tất cả";
        }

        void LoadChartData()
        {
            var db = DataProvider.Ins.DB;
            decimal revenue = 0;
            decimal debt = 0;

            if (SelectedSemester == "Tất cả" || string.IsNullOrEmpty(SelectedSemester))
            {
                // Tính toán toàn bộ
                revenue = db.Invoices.Sum(x => x.TongTienThu);
                debt = db.StudentRegistrations.Sum(x => x.TongTienHoc - x.SoTienDaDong);
            }
            else
            {
                // Tính toán theo học kỳ được chọn
                // Lưu ý: Bảng Invoice cần có cột HocKy
                revenue = db.Invoices.Where(x => x.HocKy == SelectedSemester).Sum(x => x.TongTienThu);

                debt = db.StudentRegistrations
                            .Where(x => x.HocKy == SelectedSemester)
                            .Sum(x => x.TongTienHoc - x.SoTienDaDong);
            }

            // Cập nhật biểu đồ: Vẽ 2 cột so sánh
            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Doanh Thu",
                    Values = new ChartValues<decimal> { revenue },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#4CAF50"), // Màu Xanh
                    DataLabels = true
                },
                new ColumnSeries
                {
                    Title = "Công Nợ",
                    Values = new ChartValues<decimal> { debt },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#F44336"), // Màu Đỏ
                    DataLabels = true
                }
            };

            // Trục hoành hiển thị tên học kỳ đang xem
            Labels = new[] { SelectedSemester };
        }
    }
}