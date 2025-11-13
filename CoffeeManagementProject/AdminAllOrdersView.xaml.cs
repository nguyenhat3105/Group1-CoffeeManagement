using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class AdminAllOrdersView : UserControl
    {
        private readonly OrderService _orderService;
        private List<Order> _allOrders = new();
        // private List<User> _allCustomers = new(); // <-- ĐÃ XÓA (Không cần nữa)

        public AdminAllOrdersView()
        {
            InitializeComponent();

            var ctx = new CoffeeManagementDbContext();
            // --- SỬA LỖI KHỞI TẠO (nếu bạn có nhiều service) ---
            // Bạn nên dùng chung 1 DbContext cho tất cả service trong 1 View
            var orderDao = new OrderDAO(ctx);
            var orderRepo = new OrderRepository(orderDao);
            _orderService = new OrderService(orderRepo);

            // LoadCustomers(); // <-- ĐÃ XÓA (Không cần nữa)
            LoadOrders();
        }

        /* --- HÀM LOADCUSTOMERS ĐÃ BỊ XÓA --- */

        private void LoadOrders()
        {
            try
            {
                // Tải tất cả đơn hàng (bao gồm cả thông tin Customer)
                _allOrders = _orderService.GetAllOrders(); // Giả định hàm này sẽ .Include(o => o.Customer)
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải đơn hàng: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            // Nếu _allOrders chưa tải xong, không làm gì cả
            if (_allOrders == null) return;

            IEnumerable<Order> filtered = _allOrders;

            // 1. Lấy giá trị từ các bộ lọc
            string customerSearchText = CustomerSearchBox.Text.Trim().ToLower();
            DateTime? fromDate = FromDatePicker.SelectedDate;
            DateTime? toDate = ToDatePicker.SelectedDate;

            // 2. Lọc theo Tên/Email khách hàng
            if (!string.IsNullOrEmpty(customerSearchText))
            {
                filtered = filtered.Where(o =>
                    // TH1: Đơn hàng có khách (Customer != null)
                    (o.Customer != null &&
                     (o.Customer.FullName.ToLower().Contains(customerSearchText) ||
                      o.Customer.Email.ToLower().Contains(customerSearchText))) ||

                    // TH2: Đơn hàng của khách lẻ (Customer == null) và người dùng gõ "khách lẻ"
                    (o.Customer == null && "khách lẻ".Contains(customerSearchText))
                );
            }

            // 3. Lọc theo ngày (so sánh theo .Date để bao gồm cả ngày)
            if (fromDate.HasValue)
            {
                filtered = filtered.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                // Thêm .AddDays(1) để bao gồm tất cả đơn trong ngày được chọn
                // Hoặc so sánh .Date
                filtered = filtered.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
            }

            // 4. Cập nhật danh sách hiển thị
            OrdersList.ItemsSource = filtered.OrderByDescending(o => o.CreatedAt).ToList();
        }

        // --- SỬA CÁC HÀM SỰ KIỆN ---

        // Hàm này xử lý sự kiện "SelectedDateChanged" từ DatePicker
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Hàm này (cùng tên) xử lý "TextChanged" từ TextBox
        // Đây gọi là "Method Overloading" (Nạp chồng phương thức)
        private void Filter_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;

            // Sửa: Xóa text trong TextBox thay vì ComboBox
            CustomerSearchBox.Text = string.Empty;

            // Tải lại toàn bộ đơn hàng
            LoadOrders();
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Order order)
            {
                // Giả định bạn có một Dialog tên là OrderDetailDialog
                var dlg = new OrderDetailDialog(order);
                dlg.ShowDialog();

                // Sau khi Dialog đóng, tải lại đơn hàng
                // phòng trường hợp Admin có thay đổi (ví dụ: hủy đơn)
                LoadOrders();
            }
        }
    }
}