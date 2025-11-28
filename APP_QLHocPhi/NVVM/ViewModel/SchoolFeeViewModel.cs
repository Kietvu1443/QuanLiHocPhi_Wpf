using APP_QLHocPhi.Models;
using APP_QLHocPhi.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace APP_QLHocPhi.NVVM.ViewModel
{
    // Class hiển thị thông tin học phí của từng sinh viên
    public class StudentFeeItem : BaseViewModel
    {
        public Student StudentInfo { get; set; }

        public string DisplayName { get => StudentInfo?.DisplayName; }
        public string Lop { get => StudentInfo?.Lop; }
        public string MSSV { get => StudentInfo?.Id; }
        public string SDT { get => StudentInfo?.Phone; }

        public decimal TongTienHoc { get; set; }
        public decimal DaDong { get; set; }
        public decimal ConNo { get => TongTienHoc - DaDong; }

        // Trạng thái: Nếu nợ > 0 thì là "Chưa hoàn thành"
        public string TrangThai { get => ConNo > 0 ? "Chưa hoàn thành" : "Đã hoàn thành"; }
    }

    public class SchoolFeeViewModel : BaseViewModel
    {
        // Danh sách sinh viên và công nợ
        private ObservableCollection<StudentFeeItem> _ListStudentFee;
        public ObservableCollection<StudentFeeItem> ListStudentFee { get => _ListStudentFee; set { _ListStudentFee = value; OnPropertyChanged(); } }

        // Sinh viên đang được chọn để đóng tiền
        private StudentFeeItem _SelectedItem;
        public StudentFeeItem SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    // Tự động điền số tiền còn nợ vào ô nhập
                    PaymentAmount = SelectedItem.ConNo;
                }
            }
        }

        // Số tiền muốn đóng
        private decimal _PaymentAmount;
        public decimal PaymentAmount { get => _PaymentAmount; set { _PaymentAmount = value; OnPropertyChanged(); } }

        // Ghi chú hóa đơn
        private string _PaymentNote;
        public string PaymentNote { get => _PaymentNote; set { _PaymentNote = value; OnPropertyChanged(); } }

        public ICommand PayCommand { get; set; } // Nút Thanh Toán
        public ICommand RefreshCommand { get; set; } // Nút Làm mới

        public ICommand QRCommand { get; set; } // Nút QR Code

        public ICommand ResetCommand { get; set; } // Nút Đặt lại

        public ICommand RemoveCourseCommand { get; set; } // Nút Xoá nợ (nếu cần)

        public ICommand PrintCommand { get; set; } // Nút In hoá đơn



        public SchoolFeeViewModel()
        {
            LoadData();

            //LỆNH THANH TOÁN
            PayCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện: Phải chọn SV, Tiền đóng > 0 và <= Tiền nợ
                if (SelectedItem == null) return false;
                if (PaymentAmount <= 0) return false;
                if (PaymentAmount > SelectedItem.ConNo) return false; // Không cho đóng dư
                return true;
            }, (p) =>
            {
                ProcessPayment();
                DataProvider.Ins.RefreshDataBase();
            });

            //LỆNH LÀM MỚI
            RefreshCommand = new RelayCommand<object>((p) => true, (p) => LoadData());

            QRCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện giống hệt nút Thanh Toán thường
                if (SelectedItem == null) return false;
                if (PaymentAmount <= 0) return false;
                return true;
            }, (p) =>
            {
                //CẤU HÌNH TÀI KHOẢN NGÂN HÀNG
                string bankId = "MB";       // Mã ngân hàng
                string accountNo = "0999999999"; // Số tài khoản người nhận
                string template = "compact"; // Giao diện QR (compact, print, qr_only)
                                             

                long amount = (long)PaymentAmount;
                string content = $"HP {SelectedItem.StudentInfo.Id} {SelectedItem.StudentInfo.DisplayName}";

                // Xử lý tiếng Việt có dấu thành không dấu để tránh lỗi URL 
      
                string addInfo = Uri.EscapeDataString(content);

                // Tạo link API VietQR
                string url = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={addInfo}";

                // Mở cửa sổ QR
                QRWindow qrWindow = new QRWindow(url);
                qrWindow.ShowDialog();
            });

            // nút RESET (Hủy đóng tiền)
            ResetCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện: Phải chọn sinh viên mới cho reset
                return SelectedItem != null;
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var svID = SelectedItem.StudentInfo.Id;

                // Hỏi lại user có chắc không
                var result = MessageBox.Show($"Bạn có chắc muốn HỦY TOÀN BỘ lịch sử đóng tiền của sinh viên {SelectedItem.DisplayName} không?\n\nHành động này không thể hoàn tác!",
                                             "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        //Tìm tất cả hóa đơn của sinh viên này
                        var listInvoice = db.Invoices.Where(x => x.StudentId == svID).ToList();

                        // Lấy ra danh sách ID của các hóa đơn này để xóa chi tiết
                        var listInvoiceId = listInvoice.Select(x => x.Id).ToList();

                        //Xóa tất cả Chi tiết hóa đơn (InvoiceDetail) liên quan
                        var listDetail = db.InvoiceDetails.Where(x => listInvoiceId.Contains(x.InvoiceId)).ToList();
                        db.InvoiceDetails.RemoveRange(listDetail);

                        //Xóa các Hóa đơn (Invoice)
                        db.Invoices.RemoveRange(listInvoice);

                        //Reset trạng thái các môn học về "Chưa đóng"
                        var listReg = db.StudentRegistrations.Where(x => x.StudentId == svID).ToList();
                        foreach (var item in listReg)
                        {
                            item.SoTienDaDong = 0; // Về mo
                            item.TrangThai = "Chưa đóng";
                        }

                        db.SaveChanges();
                        DataProvider.Ins.RefreshDataBase();

                        MessageBox.Show("Đã hủy thanh toán thành công!");
                        LoadData(); // Load lại danh sách để thấy nợ đỏ lòm
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Có lỗi khi hủy: " + ex.Message);
                    }
                }
            });

           
            // xoá toàn bộ nợ và học phần sinh viên

            RemoveCourseCommand = new RelayCommand<object>((p) =>
            {
                // Phải chọn sinh viên mới xóa được
                return SelectedItem != null;
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var svID = SelectedItem.StudentInfo.Id;

                var result = MessageBox.Show($"CẢNH BÁO QUAN TRỌNG!\n\nBạn sắp xóa toàn bộ danh sách môn học đã đăng ký của sinh viên {SelectedItem.DisplayName}.\n\nSau khi xóa, sinh viên này sẽ không còn môn nào trong hệ thống.",
                                             "Xác nhận hủy học phần", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Dọn dẹp Hóa đơn cũ (Bắt buộc phải xóa trước nếu có)
                        var listInvoice = db.Invoices.Where(x => x.StudentId == svID).ToList();
                        if (listInvoice.Count > 0)
                        {
                            var listInvoiceId = listInvoice.Select(x => x.Id).ToList();
                            var listDetail = db.InvoiceDetails.Where(x => listInvoiceId.Contains(x.InvoiceId)).ToList();

                            db.InvoiceDetails.RemoveRange(listDetail);
                            db.Invoices.RemoveRange(listInvoice);
                        }

                        // Xóa Đăng ký môn học (Mục tiêu chính)
                        var listReg = db.StudentRegistrations.Where(x => x.StudentId == svID).ToList();
                        db.StudentRegistrations.RemoveRange(listReg);

                        db.SaveChanges();
                        DataProvider.Ins.RefreshDataBase();

                        MessageBox.Show("Đã xóa sạch hồ sơ đăng ký! Sinh viên này giờ trắng tinh.");

                        // Load lại dữ liệu (Lúc này SV đó sẽ biến mất khỏi danh sách thu tiền vì không còn môn nào)
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                    }
                }
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                return SelectedItem != null;
            }, (p) =>
            {
                try
                {
                    var db = DataProvider.Ins.DB;

                    decimal printAmount = 0;
                    string printNote = "";
                    string printHocKy = ""; // Khai báo biến Học kỳ
                    DateTime printDate = DateTime.Now;

                    //  IN PHIẾU DỰ THU 
                    if (PaymentAmount > 0)
                    {
                        printAmount = PaymentAmount;
                        printNote = string.IsNullOrEmpty(PaymentNote) ? "Thu học phí (Tạm tính)" : PaymentNote;

                        // Lấy thử học kỳ của môn đang nợ đầu tiên để hiển thị tạm
                        var firstDebt = db.StudentRegistrations
                            .FirstOrDefault(x => x.StudentId == SelectedItem.StudentInfo.Id && x.SoTienDaDong < x.TongTienHoc);
                        printHocKy = firstDebt != null ? firstDebt.HocKy : "Tạm tính";
                    }
                    // IN LẠI HÓA ĐƠN CŨ (Đã đóng xong)
                    else
                    {
                        var lastInvoice = db.Invoices
                            .Where(x => x.StudentId == SelectedItem.StudentInfo.Id)
                            .OrderByDescending(x => x.NgayThu)
                            .FirstOrDefault();

                        if (lastInvoice != null)
                        {
                            printAmount = lastInvoice.TongTienThu;
                            printNote = lastInvoice.GhiChu;
                            printDate = lastInvoice.NgayThu;
                            printHocKy = lastInvoice.HocKy; // Lấy học kỳ từ hóa đơn cũ
                        }
                        else
                        {
                            printAmount = SelectedItem.ConNo;
                            printNote = "Thông báo công nợ";
                            printHocKy = "Tất cả";
                        }
                    }

                    //MAPPING DỮ LIỆU
                    InvoiceTemplate invoice = new InvoiceTemplate();
                    invoice.txbTenSV.Text = SelectedItem.DisplayName;
                    invoice.txbMSSV.Text = SelectedItem.MSSV;
                    invoice.txbLop.Text = SelectedItem.Lop;

                    //GÁN HỌC KỲ VÀO MẪU IN
                    invoice.txbHocKy.Text = printHocKy;
                   

                    invoice.txbSoTien.Text = string.Format("{0:N0} VNĐ", printAmount);
                    invoice.txbNoiDung.Text = printNote;
                    invoice.txbNgayThu.Text = $"Ngày {printDate.Day} tháng {printDate.Month} năm {printDate.Year}";

                    // Hiện cửa sổ in
                    System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        invoice.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                        invoice.Arrange(new Rect(new Point(0, 0), invoice.DesiredSize));
                        printDialog.PrintVisual(invoice, "Hoa Don Hoc Phi");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi in: " + ex.Message);
                }
            });
        }

        void LoadData()
        {
            var db = DataProvider.Ins.DB;
            var students = db.Students.ToList();
            var registrations = db.StudentRegistrations.ToList();

            ListStudentFee = new ObservableCollection<StudentFeeItem>();

            foreach (var sv in students)
            {
                // Lọc ra các môn học của sinh viên này
                var regOfStudent = registrations.Where(x => x.StudentId == sv.Id).ToList();

                if (regOfStudent.Count > 0)
                {
                    decimal tongTien = regOfStudent.Sum(x => x.TongTienHoc);
                    decimal daDong = regOfStudent.Sum(x => x.SoTienDaDong);

                    ListStudentFee.Add(new StudentFeeItem
                    {
                        StudentInfo = sv,
                        TongTienHoc = tongTien,
                        DaDong = daDong
                    });
                }
            }
        }

        void ProcessPayment()
        {
            var db = DataProvider.Ins.DB;
            // Kiểm tra an toàn: Nếu chưa chọn sinh viên thì dừng
            if (SelectedItem == null) return;

            var svID = SelectedItem.StudentInfo.Id;
            decimal moneyToPay = PaymentAmount;

            //LẤY DANH SÁCH MÔN CÒN NỢ (Lấy trước để biết Học kỳ nào mà điền vào hóa đơn)
 
            var unpaidRegs = db.StudentRegistrations
                .Where(x => x.StudentId == svID && x.SoTienDaDong < x.TongTienHoc)
                .OrderBy(x => x.Id) // Ưu tiên trả môn đăng ký trước
                .ToList();

            if (unpaidRegs.Count == 0)
            {
                MessageBox.Show("Sinh viên này không còn nợ môn nào để thu!");
                return;
            }

            // Lấy học kỳ của khoản nợ đầu tiên
            string hocKyThanhToan = unpaidRegs.First().HocKy;

            //TẠO HÓA ĐƠN (INVOICE)
            var invoice = new Invoice
            {
                StudentId = svID,
                NgayThu = DateTime.Now,
                TongTienThu = moneyToPay,
                GhiChu = PaymentNote ?? "Thu học phí",
                UserId = db.Users.FirstOrDefault()?.Id,
                HocKy = hocKyThanhToan // Đã có thông tin học kỳ để điền vào
            };

            db.Invoices.Add(invoice);
            db.SaveChanges(); // Lưu Invoice để lấy ID

            //PHÂN BỔ TIỀN VÀO CÁC MÔN
            // Dùng lại biến unpaidRegs
            foreach (var reg in unpaidRegs)
            {
                if (moneyToPay <= 0) break;

                decimal debtOfThisSubject = reg.TongTienHoc - reg.SoTienDaDong;
                decimal payForThis = 0;

                if (moneyToPay >= debtOfThisSubject)
                {
                    payForThis = debtOfThisSubject;
                    reg.TrangThai = "Đã hoàn thành";
                }
                else
                {
                    payForThis = moneyToPay;
                }

                reg.SoTienDaDong += payForThis;
                moneyToPay -= payForThis;

                // Tạo chi tiết hóa đơn
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    RegistrationId = reg.Id,
                    SoTienThanhToan = payForThis
                };
                db.InvoiceDetails.Add(detail);
            }

            db.SaveChanges();
            MessageBox.Show($"Thanh toán thành công {PaymentAmount:N0} VNĐ!");

            SystemLog.Log("THU HỌC PHÍ", $"Thu {PaymentAmount:N0} VNĐ của SV {SelectedItem.DisplayName} ({SelectedItem.MSSV})");

            DataProvider.Ins.RefreshDataBase();


            // Reset giao diện
            PaymentAmount = 0;
            PaymentNote = "";
            LoadData(); // Tải lại danh sách để cập nhật số nợ mới
        }
    }
}