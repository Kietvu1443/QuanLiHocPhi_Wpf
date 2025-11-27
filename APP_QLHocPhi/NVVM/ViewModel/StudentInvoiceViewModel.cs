using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    public class StudentInvoiceViewModel : BaseViewModel
    {
        // 1. Danh sách hóa đơn tìm được
        private ObservableCollection<Invoice> _ListInvoices;
        public ObservableCollection<Invoice> ListInvoices { get => _ListInvoices; set { _ListInvoices = value; OnPropertyChanged(); } }

        // 2. Hóa đơn đang chọn để in
        private Invoice _SelectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _SelectedInvoice;
            set
            {
                _SelectedInvoice = value;
                OnPropertyChanged();
                // Khi chọn hóa đơn, có thể hiện nút In
            }
        }

        // 3. Từ khóa tìm kiếm (Tên hoặc MSSV)
        private string _SearchText;
        public string SearchText { get => _SearchText; set { _SearchText = value; OnPropertyChanged(); } }

        // --- Commands ---
        public ICommand SearchCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public StudentInvoiceViewModel(User currentUser)
        {
            ListInvoices = new ObservableCollection<Invoice>();

            // Nếu là học sinh (Role != 1), tự động điền tên/mã của họ vào ô tìm kiếm và tìm luôn
            // Giả sử bảng Student có liên kết với User, hoặc ta tìm theo User.DisplayName tạm
            if (currentUser != null && currentUser.Role != "1")
            {
                SearchText = currentUser.DisplayName; // Hoặc logic mapping từ User sang StudentId
                Search();
            }

            // Lệnh Tìm kiếm
            SearchCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Search();
            });

            // Lệnh In hóa đơn
            PrintCommand = new RelayCommand<object>((p) =>
            {
                // Chỉ in được khi đã chọn hóa đơn
                return SelectedInvoice != null;
            }, (p) =>
            {
                PrintInvoice();
            });

            // Lệnh Đóng cửa sổ
            CloseCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p?.Close(); });
        }

        void Search()
        {
            if (string.IsNullOrEmpty(SearchText)) return;

            // Tìm trong DB: Hóa đơn của sinh viên có Tên hoặc ID chứa từ khóa
            var list = DataProvider.Ins.DB.Invoices
                .Where(x => x.Student.Ten.Contains(SearchText) ||
                            x.Student.HoDem.Contains(SearchText) ||
                            x.StudentId.Contains(SearchText))
                .OrderByDescending(x => x.NgayThu) // Hóa đơn mới nhất lên đầu
                .ToList();

            ListInvoices = new ObservableCollection<Invoice>(list);
        }

        void PrintInvoice()
        {
            // Sử dụng InvoiceTemplate đã có trong project
            // Giả sử InvoiceTemplate có constructor nhận Invoice hoặc DataContext
            try
            {
                InvoiceTemplate printWindow = new InvoiceTemplate();
                // Gán dữ liệu cho cửa sổ in (bạn có thể cần tạo ViewModel cho InvoiceTemplate hoặc gán trực tiếp)
                // Ví dụ: printWindow.DataContext = SelectedInvoice; 

                // Show dialog in ấn
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // In Visual của cửa sổ InvoiceTemplate (hoặc FlowDocument nếu bạn dùng Document)
                    printDialog.PrintVisual(printWindow.Prin, "Hóa Đơn Học Phí");
                    // Lưu ý: "PrintArea" là tên Grid chính trong InvoiceTemplate.xaml
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi in ấn: " + ex.Message);
            }
        }
    }
}