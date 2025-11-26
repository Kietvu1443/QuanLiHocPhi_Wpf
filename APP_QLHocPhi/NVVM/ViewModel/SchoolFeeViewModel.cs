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

            // --- LỆNH THANH TOÁN ---
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
            });

            // --- LỆNH LÀM MỚI ---
            RefreshCommand = new RelayCommand<object>((p) => true, (p) => LoadData());

            QRCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện giống hệt nút Thanh Toán thường
                if (SelectedItem == null) return false;
                if (PaymentAmount <= 0) return false;
                return true;
            }, (p) =>
            {
                // --- CẤU HÌNH TÀI KHOẢN NGÂN HÀNG CỦA CẬU Ở ĐÂY ---
                string bankId = "MB";       // Mã ngân hàng (VD: MB, VCB, TPB, VPB...)
                string accountNo = "0999999999"; // Số tài khoản người nhận (Thay bằng STK của cậu)
                string template = "compact"; // Giao diện QR (compact, print, qr_only)
                                             // ----------------------------------------------------

                long amount = (long)PaymentAmount;
                string content = $"HP {SelectedItem.StudentInfo.Id} {SelectedItem.StudentInfo.DisplayName}";

                // Xử lý tiếng Việt có dấu thành không dấu để tránh lỗi URL (Hoặc VietQR tự xử lý, nhưng nên cẩn thận)
                // Ở đây mình dùng Uri.EscapeDataString để mã hóa nội dung cho an toàn
                string addInfo = Uri.EscapeDataString(content);

                // Tạo link API VietQR
                string url = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={addInfo}";

                // Mở cửa sổ QR
                QRWindow qrWindow = new QRWindow(url);
                qrWindow.ShowDialog();
            });

            // 2. Định nghĩa logic nút RESET (Hủy đóng tiền)
            ResetCommand = new RelayCommand<object>((p) =>
            {
                // Điều kiện: Phải chọn sinh viên mới cho reset
                return SelectedItem != null;
            }, (p) =>
            {
                var db = DataProvider.Ins.DB;
                var svID = SelectedItem.StudentInfo.Id;

                // Hỏi lại cho chắc ăn, nhỡ tay bấm nhầm thì toi
                var result = MessageBox.Show($"Bạn có chắc muốn HỦY TOÀN BỘ lịch sử đóng tiền của sinh viên {SelectedItem.DisplayName} không?\n\nHành động này không thể hoàn tác!",
                                             "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // BƯỚC 1: Tìm tất cả hóa đơn của sinh viên này
                        var listInvoice = db.Invoices.Where(x => x.StudentId == svID).ToList();

                        // Lấy ra danh sách ID của các hóa đơn này để xóa chi tiết
                        var listInvoiceId = listInvoice.Select(x => x.Id).ToList();

                        // BƯỚC 2: Xóa tất cả Chi tiết hóa đơn (InvoiceDetail) liên quan
                        var listDetail = db.InvoiceDetails.Where(x => listInvoiceId.Contains(x.InvoiceId)).ToList();
                        db.InvoiceDetails.RemoveRange(listDetail);

                        // BƯỚC 3: Xóa các Hóa đơn (Invoice)
                        db.Invoices.RemoveRange(listInvoice);

                        // BƯỚC 4: Reset trạng thái các môn học về "Chưa đóng"
                        var listReg = db.StudentRegistrations.Where(x => x.StudentId == svID).ToList();
                        foreach (var item in listReg)
                        {
                            item.SoTienDaDong = 0; // Về mo
                            item.TrangThai = "Chưa đóng";
                        }

                        db.SaveChanges();

                        MessageBox.Show("Đã hủy thanh toán thành công! Sinh viên này lại nợ như chúa Chổm rồi nha :))");
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
                        // BƯỚC 1: Dọn dẹp Hóa đơn cũ (Bắt buộc phải xóa trước nếu có)
                        var listInvoice = db.Invoices.Where(x => x.StudentId == svID).ToList();
                        if (listInvoice.Count > 0)
                        {
                            var listInvoiceId = listInvoice.Select(x => x.Id).ToList();
                            var listDetail = db.InvoiceDetails.Where(x => listInvoiceId.Contains(x.InvoiceId)).ToList();

                            db.InvoiceDetails.RemoveRange(listDetail);
                            db.Invoices.RemoveRange(listInvoice);
                        }

                        // BƯỚC 2: Xóa Đăng ký môn học (Mục tiêu chính)
                        var listReg = db.StudentRegistrations.Where(x => x.StudentId == svID).ToList();
                        db.StudentRegistrations.RemoveRange(listReg);

                        db.SaveChanges();

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

                    // Khai báo các biến để hứng dữ liệu in
                    decimal printAmount = 0;
                    string printNote = "";
                    DateTime printDate = DateTime.Now;

                    // --- LOGIC XỬ LÝ SỐ TIỀN IN ---

                    // TRƯỜNG HỢP 1: Đang nhập số tiền vào ô (Chưa bấm thanh toán) 
                    // -> In phiếu dự thu / báo giá
                    if (PaymentAmount > 0)
                    {
                        printAmount = PaymentAmount;
                        printNote = string.IsNullOrEmpty(PaymentNote) ? "Thu học phí (Tạm tính)" : PaymentNote;
                        printDate = DateTime.Now;
                    }
                    // TRƯỜNG HỢP 2: Ô nhập tiền = 0 (Thường là đã bấm thanh toán xong rồi)
                    // -> Tìm hóa đơn GẦN NHẤT trong lịch sử để in lại
                    else
                    {
                        var lastInvoice = db.Invoices
                            .Where(x => x.StudentId == SelectedItem.StudentInfo.Id)
                            .OrderByDescending(x => x.NgayThu) // Sắp xếp ngày mới nhất lên đầu
                            .FirstOrDefault(); // Lấy cái đầu tiên

                        if (lastInvoice != null)
                        {
                            // Lấy thông tin từ hóa đơn cũ
                            printAmount = lastInvoice.TongTienThu;
                            printNote = lastInvoice.GhiChu;
                            printDate = lastInvoice.NgayThu;
                        }
                        else
                        {
                            // Nếu chưa từng đóng đồng nào -> In phiếu báo tổng nợ
                            printAmount = SelectedItem.ConNo;
                            printNote = "Thông báo công nợ";
                        }
                    }

                    // --- MAPPING DỮ LIỆU VÀO MẪU IN (Giữ nguyên phần này) ---
                    InvoiceTemplate invoice = new InvoiceTemplate();
                    invoice.txbTenSV.Text = SelectedItem.DisplayName;
                    invoice.txbMSSV.Text = SelectedItem.MSSV;
                    invoice.txbLop.Text = SelectedItem.Lop;

                    // Gán dữ liệu đã xử lý ở trên vào
                    invoice.txbSoTien.Text = string.Format("{0:N0} VNĐ", printAmount);
                    invoice.txbNoiDung.Text = printNote;
                    invoice.txbNgayThu.Text = $"Ngày {printDate.Day} tháng {printDate.Month} năm {printDate.Year}";

                    // --- HIỆN CỬA SỔ IN ---
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

            // --- BƯỚC 1: LẤY DANH SÁCH MÔN CÒN NỢ ---
            // (Lấy trước để biết Học kỳ nào mà điền vào hóa đơn)
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

            // --- BƯỚC 2: TẠO HÓA ĐƠN (INVOICE) ---
            var invoice = new Invoice
            {
                StudentId = svID,
                NgayThu = DateTime.Now,
                TongTienThu = moneyToPay,
                GhiChu = PaymentNote ?? "Thu học phí",
                UserId = db.Users.FirstOrDefault()?.Id,
                HocKy = hocKyThanhToan // <--- Đã có thông tin học kỳ để điền vào
            };

            db.Invoices.Add(invoice);
            db.SaveChanges(); // Lưu Invoice để lấy ID

            // --- BƯỚC 3: PHÂN BỔ TIỀN VÀO CÁC MÔN ---
            // (Dùng lại biến unpaidRegs đã lấy ở Bước 1, không khai báo lại nữa)
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

            // Reset giao diện
            PaymentAmount = 0;
            PaymentNote = "";
            LoadData(); // Tải lại danh sách để cập nhật số nợ mới
        }
    }
}