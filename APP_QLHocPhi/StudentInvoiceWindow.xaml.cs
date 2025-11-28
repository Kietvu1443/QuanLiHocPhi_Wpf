using APP_QLHocPhi.Models;
using APP_QLHocPhi.NVVM.ViewModel; 
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi
{
    /// <summary>
    /// Interaction logic for StudentInvoiceWindow.xaml
    /// </summary>
    public partial class StudentInvoiceWindow : Window
    {
     
        public StudentInvoiceWindow(User user)
        {
            InitializeComponent();

            //Gán DataContext: Tạo ViewModel và truyền User vào
            this.DataContext = new StudentInvoiceViewModel(user);
        }
    }
}