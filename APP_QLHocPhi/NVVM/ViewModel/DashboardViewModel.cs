using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class DashboardViewModel : BaseViewModel
    {
        private int _TotalStudents;
        public int TotalStudent { get => _TotalStudents; set { _TotalStudents = value; OnPropertyChanged(); } }

        private int _PaidCount;
        public int PaidCount { get => _PaidCount; set {_PaidCount = value; OnPropertyChanged();} }

        private int _UnPaidCount;
        public int UnPaidCount { get => _UnPaidCount; set { _UnPaidCount = value; OnPropertyChanged(); } }

        private ObservableCollection<Student> _List;
        public ObservableCollection<Student> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        public DashboardViewModel()
        {
            LoadDashBoardData();
        }
        void LoadDashBoardData()
        {
            TotalStudent = DataProvider.Ins.DB.Students.Count(); // Tổng số học sinh

            PaidCount = DataProvider.Ins.DB.Students.Where(x => x.TrangThai == "Đã đóng").Count(); // Số học sinh đã đóng học phí

            UnPaidCount = TotalStudent - PaidCount; // Số học sinh chưa đóng học phí

            List = new ObservableCollection<Student>(DataProvider.Ins.DB.Students);
        }
    }
}
