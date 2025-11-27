using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class StudentInvoiceViewModel : BaseViewModel
    {
        // Danh sách hóa đơn tìm được
        private ObservableCollection<Invoice> _ListInvoices;
        public ObservableCollection<Invoice> ListInvoices { get => _ListInvoices; set { _ListInvoices = value; OnPropertyChanged(); } }

        // Hóa đơn đang được chọn
        private Invoice _SelectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _SelectedInvoice;
            set { _SelectedInvoice = value; OnPropertyChanged(); }
        }

        // Từ khóa tìm kiếm
        private string _SearchText;
        public string SearchText { get => _SearchText; set { _SearchText = value; OnPropertyChanged(); } }

        public ICommand SearchCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public StudentInvoiceViewModel(User currentUser)
        {
            ListInvoices = new ObservableCollection<Invoice>();

            // Nếu là học sinh, tự động điền tên để tìm luôn cho tiện
            if (currentUser != null && currentUser.Role != "1")
            {
                SearchText = currentUser.DisplayName;
                Search();
            }

            // Command Tìm kiếm
            SearchCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Search();
            });

            // Command In
            PrintCommand = new RelayCommand<object>((p) =>
            {
                // Chỉ cho in khi đã chọn hóa đơn
                return SelectedInvoice != null;
            }, (p) =>
            {
                PrintInvoice();
            });

            // Command Đóng cửa sổ
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p?.Close(); });
        }

        void Search()
        {
            if (string.IsNullOrEmpty(SearchText)) return;

            // Logic tìm kiếm: Tìm theo Tên SV, Họ Đệm hoặc Mã SV
            // Dùng Include("Student") để lấy luôn thông tin sinh viên đi kèm hóa đơn
            var list = DataProvider.Ins.DB.Invoices
                        .Include(x => x.Student)
                        .Where(x => x.Student.Ten.Contains(SearchText) ||
                                    x.Student.HoDem.Contains(SearchText) ||
                                    x.StudentId.Contains(SearchText))
                        .OrderByDescending(x => x.NgayThu)
                        .ToList();

            ListInvoices = new ObservableCollection<Invoice>(list);
        }

        void PrintInvoice()
        {
            try
            {
                // Gọi cửa sổ mẫu in (InvoiceTemplate)
                InvoiceTemplate printWindow = new InvoiceTemplate();

                // Truyền dữ liệu hóa đơn vào DataContext của mẫu in
                printWindow.DataContext = SelectedInvoice;

                // Hiện hộp thoại in của Windows
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Ẩn các nút không cần thiết trên template trước khi in (nếu có)
                    // Ở đây ta in nguyên cái Grid chính của Window đó
                    printDialog.PrintVisual(printWindow.Content as System.Windows.Media.Visual, "Hóa Đơn Học Phí");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Có lỗi khi in: " + ex.Message);
            }
        }
    }
}