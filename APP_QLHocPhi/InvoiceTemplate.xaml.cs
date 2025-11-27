using APP_QLHocPhi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APP_QLHocPhi
{
    /// <summary>
    /// Interaction logic for InvoiceTemplate.xaml
    /// </summary>
    public partial class InvoiceTemplate : UserControl
    {
        public InvoiceTemplate()
        {
            InitializeComponent();
        }
        public void SetInvoiceData(Invoice invoice)
        {
            if (invoice == null) return;

            if (invoice.Student != null)
            {
                txbTenSV.Text = invoice.Student.DisplayName;
                txbLop.Text = invoice.Student.Lop;
            }

            txbMSSV.Text = invoice.StudentId;

            // --- THÊM DÒNG NÀY ---
            txbHocKy.Text = invoice.HocKy;
            // ---------------------

            txbSoTien.Text = string.Format("{0:N0} VNĐ", invoice.TongTienThu);
            txbNoiDung.Text = invoice.GhiChu;
            txbNgayThu.Text = $"Ngày {invoice.NgayThu.Day} tháng {invoice.NgayThu.Month} năm {invoice.NgayThu.Year}";
        }
    }
}
