using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.Helpers;
using System;
using System.Collections.Generic; // Cần cho List (mặc dù không dùng trực tiếp)
using System.IO;                   // Cần cho Path, File, FileMode, FileAccess, FileShare
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging; // Cần cho BitmapImage

namespace CoffeeManagement
{
    public partial class MainWindow : Window
    {
        private readonly IAuthorizationService _auth;
        private readonly string AvatarFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CoffeeManagement",
            "UserAvatars");

        public MainWindow()
        {
            InitializeComponent();
            _auth = new AuthorizationService();

            // Gọi hàm Setup TỔNG HỢP
            SetupByRole();

            LoadControl("MenuUI"); // default view
        }

        /// <summary>
        /// Hàm này đã được hợp nhất từ 2 hàm SetupByRole và SetupByRoles
        /// </summary>
        private void SetupByRole()
        {
            var user = AppSession.CurrentUser;

            // --- 1. Đặt thông báo Chào mừng (Thanh trên cùng) ---
            

            // --- 2. Đặt thông tin Nav (Góc dưới bên trái) ---
            if (user != null)
            {
                // Đặt Tên và Vai trò
                NavUserName.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
                NavUserRole.Text = (user.RoleId == 1) ? "Admin" : (user.RoleId == 2 ? "Staff" : "Customer");

                // Tải Avatar
                try
                {
                    string fileName = $"user_{user.Id}.jpg"; // Giả định tên file avatar
                    string avatarPath = Path.Combine(AvatarFolder, fileName);

                    if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
                    {
                        var bmp = new BitmapImage();
                        // Dùng FileStream và FileShare.Read để tránh khóa file
                        using (var stream = new FileStream(avatarPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad; // Tải vào bộ nhớ
                            bmp.StreamSource = stream;
                            bmp.EndInit();
                            bmp.Freeze(); // Quan trọng cho WPF
                        }
                        NavAvatarImage.Source = bmp;
                    }
                    // else: Sẽ tự động hiển thị Ellipse (vòng tròn) placeholder
                }
                catch (Exception ex)
                {
                    // (Tùy chọn) Ghi log nếu tải avatar thất bại
                    System.Diagnostics.Debug.WriteLine($"Failed to load avatar: {ex.Message}");
                }

                // --- 3. Cài đặt Ẩn/Hiện nút theo Vai trò ---
                if (user.RoleId == 1) // Admin
                {
                    // Admin: Thấy tất cả (không cần ẩn gì)
                }
                else if (user.RoleId == 2) // Staff
                {
                    // Staff: Ẩn các nút quản lý của Admin
                    BtnUsers.Visibility = Visibility.Collapsed; // (Giả định Staff không quản lý Users)
                    BtnMenu.Visibility = Visibility.Collapsed; // (Giả định Staff dùng MenuUI)
                    BtnAdminDashboard.Visibility = Visibility.Collapsed;
                    BtnOrderHistory.Visibility = Visibility.Collapsed;
                    // (Giữ lại BtnAdminOrders, BtnMenuUI, BtnProfile, BtnOrders, BtnOrderHistory)
                }
                else if (user.RoleId == 3) // Customer
                {
                    // Customer: Chỉ thấy các nút của khách hàng
                    BtnUsers.Visibility = Visibility.Collapsed;
                    BtnMenu.Visibility = Visibility.Collapsed;
                    BtnOrders.Visibility = Visibility.Collapsed;
                    BtnAdminOrders.Visibility = Visibility.Collapsed;
                    BtnAdminDashboard.Visibility = Visibility.Collapsed;
                    BtnPromotion.Visibility = Visibility.Collapsed;
                    // (Giữ lại BtnMenuUI, BtnProfile, BtnOrderHistory)
                }
            }
            else // Nếu không có ai đăng nhập (Guest)
            {
                NavUserName.Text = "Guest";
                NavUserRole.Text = "Not Logged In";
                // Ẩn tất cả các nút (chỉ để lại MenuUI và Logout?)
                // (Tùy theo logic của bạn, ở đây tôi ẩn đa số các nút)
                BtnUsers.Visibility = Visibility.Collapsed;
                BtnProfile.Visibility = Visibility.Collapsed;
                BtnOrders.Visibility = Visibility.Collapsed;
                BtnAdminOrders.Visibility = Visibility.Collapsed;
                BtnAdminDashboard.Visibility = Visibility.Collapsed;
                BtnOrderHistory.Visibility = Visibility.Collapsed;
                BtnPromotion.Visibility = Visibility.Collapsed;
                BtnMenu.Visibility = Visibility.Collapsed;
            }
        }

        // --- CÁC HÀM XỬ LÝ SỰ KIỆN ---

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                switch (b.Name)
                {
                    // (Logic điều hướng của bạn đã đúng)
                    case "BtnUsers": LoadControl("Users"); break;
                    case "BtnMenu": LoadControl("Menu"); break;
                    case "BtnOrders": LoadControl("Orders"); break;
                    case "BtnAdminOrders": LoadControl("AdminOrders"); break;
                    case "BtnAdminDashboard": LoadControl("AdminDashboard"); break;
                    case "BtnProfile": LoadControl("Profile"); break;
                    case "BtnMenuUI": LoadControl("MenuUI"); break;
                    case "BtnOrderHistory": LoadControl("OrderHistory"); break;
                    case "BtnPromotion": LoadControl("Promotion"); break;
                }
            }
        }

        private void LoadControl(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "MenuUI"; // Đặt MenuUI làm mặc định

            UserControl uc;
            switch (name.Trim())
            {
                // (Logic tải Control của bạn đã đúng)
                case "Users": uc = new UsersListControl(); break;
                case "Menu": uc = new MenuItemsList(); break;
                case "MenuUI": uc = new MenuUI(); break;
                case "Orders": uc = new StaffOrders(); break;
                case "AdminOrders": uc = new AdminAllOrdersView(); break;
                case "AdminDashboard": uc = new AdminDashboardView(); break;
                case "Profile": uc = new UserProfile(); break;
                case "OrderHistory": uc = new OrderHistory(); break;
                case "Promotion": uc = new AdminPromotionsView(); break;
                default: uc = new MenuUI(); break; // Mặc định là MenuUI
            }

            ContentRegion.Content = uc;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AppSession.Clear();
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // --- CÁC HÀM ĐIỀU KHIỂN CỬA SỔ ---

        // 1. Cho phép kéo thả cửa sổ
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // 2. Nút Thu nhỏ
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 3. Nút Phóng to / Khôi phục
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                (sender as Button).Content = "\uE923"; // Icon Khôi phục
            }
            else
            {
                this.WindowState = WindowState.Normal;
                (sender as Button).Content = "\uE922"; // Icon Phóng to
            }
        }

        // 4. Nút Đóng
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // *** HÀM 'SetupByRoles' (SỐ NHIỀU) ĐÃ BỊ XÓA VÌ ĐÃ HỢP NHẤT ***
    }
}