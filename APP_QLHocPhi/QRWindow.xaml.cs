using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace APP_QLHocPhi
{
    public partial class QRWindow : Window
    {
        public QRWindow(string qrUrl)
        {
            InitializeComponent();
            LoadQrImage(qrUrl);
        }

        void LoadQrImage(string url)
        {
            try
            {
                // Tạo ảnh từ đường dẫn URL
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();
                QrImage.Source = bitmap;
            }
            catch (Exception)
            {
                MessageBox.Show("Không tải được mã QR. Vui lòng kiểm tra kết nối mạng!");
            }
        }
    }
}