using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    // Tạo một class Wrapper để hiển thị lên View
    // Class này giúp ta có thêm STT mà không đụng vào Model gốc
    public class StudentDisplay
    {
        public int STT { get; set; }
        public Student StudentInfo { get; set; } 

        
        public string Id { get => StudentInfo.Id; }
        public string HoDem { get => StudentInfo.HoDem; }
        public string Ten { get => StudentInfo.Ten; }
        public string Lop { get => StudentInfo.Lop; }
        public string Nganh { get => StudentInfo.Nganh; }
        public string QRcode { get; set; } // noted : sẽ thêm vào sau
    }

    public class DashboardViewModel : BaseViewModel
    {
        private int _TotalStudents;
        public int TotalStudent { get => _TotalStudents; set { _TotalStudents = value; OnPropertyChanged(); } }

        private int _PaidCount;
        public int PaidCount { get => _PaidCount; set { _PaidCount = value; OnPropertyChanged(); } }

        private int _UnPaidCount;
        public int UnPaidCount { get => _UnPaidCount; set { _UnPaidCount = value; OnPropertyChanged(); } }

        private decimal _TongDoanhThu;
        public decimal TongDoanhThu { get => _TongDoanhThu; set { _TongDoanhThu = value; OnPropertyChanged(); } }

        private decimal _TongCongNo;
        public decimal TongCongNo { get => _TongCongNo; set { _TongCongNo = value; OnPropertyChanged(); } }

        // --- DỮ LIỆU BIỂU ĐỒ ---
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }



        //Sửa kiểu dữ liệu của List thành StudentDisplay
        private ObservableCollection<StudentDisplay> _List;
        public ObservableCollection<StudentDisplay> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        public DashboardViewModel()
        {
            LoadDashBoardData();

            DataProvider.Ins.DatabaseChanged += () =>
            {
                LoadDashBoardData();
            };
        }

        void LoadDashBoardData()
        {
            var db = DataProvider.Ins.DB; // Lấy DB context
            var allRegs = db.StudentRegistrations.ToList();//Tính tiền (Gán vào biến mới)

            // Tính tổng tiền
            decimal daThu = allRegs.Sum(x => x.SoTienDaDong);
            decimal tongPhaiThu = allRegs.Sum(x => x.TongTienHoc);
            decimal conNo = tongPhaiThu - daThu;

            // Gán dữ liệu hiển thị
            TongDoanhThu = daThu;
            TongCongNo = conNo;

            TotalStudent = db.Students.Count();
            PaidCount = db.Students.Where(x => x.TrangThai == "Đã đóng").Count();
            UnPaidCount = TotalStudent - PaidCount;

            // Lấy danh sách gốc từ DB
            var studentListFromDB = db.Students.ToList();

            //Chuyển đổi sang danh sách hiển thị và đánh số STT
            var displayList = new ObservableCollection<StudentDisplay>();
            int i = 1;

            foreach (var item in studentListFromDB)
            {
                displayList.Add(new StudentDisplay
                {
                    STT = i, // Gán số thứ tự tăng dần
                    StudentInfo = item, // Lưu thông tin gốc
                    QRcode = "" // sẽ thêm vào sau
                });
                i++;
            }

            List = displayList;

            Formatter = value => value.ToString("N0") + " đ";

            // Cấu hình Biểu đồ CỘT (Column Chart)
            // Trục Y hiện tiền Việt Nam
            SeriesCollection = new SeriesCollection
            {
                // Cột 1: Tiền đã thu (Màu Xanh)
                new ColumnSeries
                {
                    Title = "Đã Thu",
                    Values = new ChartValues<decimal> { daThu },
                    Fill = System.Windows.Media.Brushes.Green,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0")
                },
                // Cột 2: Tiền còn nợ (Màu Đỏ)
                new ColumnSeries
                {
                    Title = "Còn Nợ",
                    Values = new ChartValues<decimal> { conNo },
                    Fill = System.Windows.Media.Brushes.Red, //Red
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("N0")
                }
            };

            Labels = new[] { "Tổng quan tài chính" };
        }
    }
}