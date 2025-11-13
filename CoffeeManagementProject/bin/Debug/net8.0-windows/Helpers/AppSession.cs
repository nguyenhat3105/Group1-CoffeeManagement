using CoffeeManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.Helpers
{
    public static class AppSession
    {
        // Lưu user đang đăng nhập
        public static User? CurrentUser { get; private set; }

        // Gán thông tin khi đăng nhập thành công
        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        // Xóa thông tin khi đăng xuất
        public static void Clear()
        {
            CurrentUser = null;
        }


    }
}
