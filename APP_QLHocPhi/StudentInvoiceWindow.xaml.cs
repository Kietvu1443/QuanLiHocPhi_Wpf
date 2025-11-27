using APP_QLHocPhi.Models; // Cần dùng Model User
using APP_QLHocPhi.NVVM.ViewModel; // Cần dùng ViewModel
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi
{
    /// <summary>
    /// Interaction logic for StudentInvoiceWindow.xaml
    /// </summary>
    public partial class StudentInvoiceWindow : Window
    {
        // 1. Sửa Constructor: Thêm tham số User vào đây
        public StudentInvoiceWindow(User user)
        {
            InitializeComponent();

            // 2. Gán DataContext: Tạo ViewModel và truyền User vào
            this.DataContext = new StudentInvoiceViewModel(user);
        }
    }
}