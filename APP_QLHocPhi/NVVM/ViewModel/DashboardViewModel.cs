using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    // 1. Tạo một class Wrapper để hiển thị lên View
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

        // 2. Sửa kiểu dữ liệu của List thành StudentDisplay
        private ObservableCollection<StudentDisplay> _List;
        public ObservableCollection<StudentDisplay> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        public DashboardViewModel()
        {
            LoadDashBoardData();
        }

        void LoadDashBoardData()
        {
            var db = DataProvider.Ins.DB; // Lấy DB context

            TotalStudent = db.Students.Count();
            PaidCount = db.Students.Where(x => x.TrangThai == "Đã đóng").Count();
            UnPaidCount = TotalStudent - PaidCount;

            // Lấy danh sách gốc từ DB
            var studentListFromDB = db.Students.ToList();

            // 3. Chuyển đổi sang danh sách hiển thị và đánh số STT
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
        }
    }
}