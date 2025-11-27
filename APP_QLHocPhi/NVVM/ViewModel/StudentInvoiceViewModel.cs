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

            // 1. ĐĂNG KÝ SỰ KIỆN: Khi Database thay đổi -> Tự động chạy lại hàm Search
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

            // 2. HỦY ĐĂNG KÝ KHI ĐÓNG: Quan trọng để tránh lỗi và tốn Ram
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) =>
            {
                // Trước khi đóng cửa sổ, ta "rút dây" lắng nghe ra
                DataProvider.Ins.DatabaseChanged -= Search;
                p?.Close();
            });
        }

        void Search()
        {
            // Kiểm tra null để tránh lỗi nếu sự kiện bắn ra lúc chưa có SearchText
            if (string.IsNullOrEmpty(SearchText)) return;

            // Dùng Application.Current.Dispatcher để đảm bảo chạy trên luồng UI (an toàn tuyệt đối)
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Lấy lại dữ liệu mới nhất từ DB
                var list = DataProvider.Ins.DB.Invoices
                            .Include(x => x.Student)
                            .Where(x => x.Student.Ten.Contains(SearchText) ||
                                        x.Student.HoDem.Contains(SearchText) ||
                                        x.StudentId.Contains(SearchText))
                            .OrderByDescending(x => x.NgayThu)
                            .ToList();

                ListInvoices = new ObservableCollection<Invoice>(list);
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