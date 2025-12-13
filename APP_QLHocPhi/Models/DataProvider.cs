using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_QLHocPhi.Models
{
    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins { get { if (_ins == null) _ins = new DataProvider(); return _ins; } set { _ins = value; } } 

        public QuanLiHocPhiContext DB { get; set; }

        //Tạo ra một lớp để ánh xạ truy cập database dễ dàng hơn thông qua DataProvider
        //DataProvider.Ins.DB.Students 




        // Tạo sự kiện để các window khác lắng nghe
        public event Action DatabaseChanged;
        // Hàm để kích hoạt sự kiện
        public void RefreshDataBase()
        {
            // Khi gọi hàm này, ai đang lắng nghe sẽ tự động chạy
            DatabaseChanged?.Invoke();
        }

        private DataProvider()
        {
            DB = new QuanLiHocPhiContext();
        }
    }
}
