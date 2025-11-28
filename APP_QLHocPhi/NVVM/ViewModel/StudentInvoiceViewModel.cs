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
        public string SearchText { get => _SearchText; set { _SearchText = value; OnPropertyChanged(); Search(); } }

        public ICommand SearchCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public StudentInvoiceViewModel(User currentUser)
        {
            ListInvoices = new ObservableCollection<Invoice>();

            //ĐĂNG KÝ SỰ KIỆN: Khi Database thay đổi -> Tự động chạy lại hàm Search
            DataProvider.Ins.DatabaseChanged += Search;

            if (currentUser != null && currentUser.Role != "1")
            {
                SearchText = currentUser.DisplayName;
                Search();
            }

            SearchCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Search();
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                return SelectedInvoice != null;
            }, (p) =>
            {
                PrintInvoice();
            });

            //HỦY ĐĂNG KÝ KHI ĐÓNG: Quan trọng để tránh lỗi và tốn Ram
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                // Trước khi đóng cửa sổ, ta "rút dây" lắng nghe ra
                DataProvider.Ins.DatabaseChanged -= Search;
                p?.Close();
            });
        }

        void Search()
        {
            if (string.IsNullOrEmpty(SearchText)) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // QUAN TRỌNG: Tạo 'using' context mới để ép buộc tải lại dữ liệu từ Database
                // Thay vì dùng DataProvider.Ins.DB (cũ), ta dùng 'new QuanLiHocPhiContext()'
                using (var context = new QuanLiHocPhiContext())
                {
                    var list = context.Invoices
                                .Include(x => x.Student) // Nhớ Include Sinh viên
                                .Where(x => x.Student.Ten.Contains(SearchText) ||
                                            x.Student.HoDem.Contains(SearchText) ||
                                            x.StudentId.Contains(SearchText))
                                .OrderByDescending(x => x.NgayThu)
                                .ToList();

                    // Gán vào ObservableCollection
                    ListInvoices = new ObservableCollection<Invoice>(list);
                }
            });
        }

        void PrintInvoice()
        {
            try
            {
                if (SelectedInvoice == null) return;

                //Tạo mẫu in
                InvoiceTemplate invoiceTemplate = new InvoiceTemplate();

                //GỌI HÀM GÁN DỮ LIỆU VỪA TẠO (Đây là chìa khóa!)
                // Cách này ép giao diện phải nhận dữ liệu ngay lập tức
                invoiceTemplate.SetInvoiceData(SelectedInvoice);

                //Chuẩn bị in
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Cập nhật lại layout lần cuối cho chắc ăn
                    // Tính toán kích thước cần thiết để hiển thị hết nội dung
                    invoiceTemplate.Measure(new Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                    invoiceTemplate.Arrange(new Rect(new Point(0, 0), invoiceTemplate.DesiredSize));
                    invoiceTemplate.UpdateLayout(); // Bắt buộc vẽ lại giao diện

                    // Thực hiện in
                    printDialog.PrintVisual(invoiceTemplate, "Hóa Đơn Học Phí - " + SelectedInvoice.StudentId);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Có lỗi khi in: " + ex.Message);
            }
        }
    }
}