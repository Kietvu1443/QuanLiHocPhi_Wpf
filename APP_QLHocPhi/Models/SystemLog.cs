using System;
using System.IO;

namespace APP_QLHocPhi.Models
{
    public static class SystemLog
    {
        // Tạo đường dẫn vào thư mục AppData của máy tính
        private static string GetLogPath()
        {
            // Lấy đường dẫn thư mục AppData
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Tạo thư mục riêng cho App của mình
            string folderPath = Path.Combine(appData, "QuanLiHocPhi");

            // Nếu chưa có thì tạo mới
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Trả về đường dẫn file đầy đủ
            return Path.Combine(folderPath, "NhatKiHeThong.txt");
        }

        public static void Log(string action, string detail)
        {
            try
            {
                var user = UserSession.CurrentUser;
                string userName = user != null ? user.DisplayName : "Unknown";
                string role = (user != null && user.Role == "1") ? "Admin" : "User";
                string time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                string logContent = $"[{time}] - [{role}: {userName}] - {action.ToUpper()} - {detail}\n";

                //Ghi vào đường dẫn chuẩn
                File.AppendAllText(GetLogPath(), logContent);
            }
            catch (Exception)
            {
                // Im lặng nếu lỗi, không làm phiền người dùng
            }
        }

        // Hàm hỗ trợ mở file log nhanh để xem (nếu cần dùng sau này)
        public static string GetCurrentLogPath() => GetLogPath();
    }
}