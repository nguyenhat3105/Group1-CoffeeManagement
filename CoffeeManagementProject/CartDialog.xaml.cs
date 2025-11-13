using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.Helpers;
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

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for CartDialog.xaml
    /// </summary>
    public partial class CartDialog : Window
    {
        private List<CartItem> _cart;

        public CartDialog(List<CartItem> cartItems)
        {
            InitializeComponent();

            // Gộp trùng món (nếu user nhấn "Add to cart" nhiều lần)
            _cart = cartItems
                .GroupBy(i => i.Item.Id)
                .Select(g => new CartItem
                {
                    Item = g.First().Item,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            CartList.ItemsSource = _cart;
            UpdateTotal();
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CartItem item)
            {
                item.Quantity++;
                CartList.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CartItem item)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    _cart.Remove(item);

                CartList.Items.Refresh();
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            TxtTotal.Text = $"{_cart.Sum(i => i.TotalPrice):C0}";
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Chuẩn bị Order và OrderItems
                var order = new Order
                {
                    CustomerId = AppSession.CurrentUser?.Id, // nếu user đã login
                    StaffId = null, // staff sẽ nhận sau
                    Status = 0,     // 0 = processing
                    IsPaid = false,
                    CreatedAt = DateTime.Now,
                    TotalAmount = _cart.Sum(i => i.TotalPrice)
                };

                var orderItems = _cart.Select(ci => new OrderItem
                {
                    MenuItemId = ci.Item.Id,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Item.Price
                }).ToList();

                // 2. Tạo DAO/Repo/Service (không DI)
                using var ctx = new CoffeeManagementDbContext();
                var dao = new OrderDAO(ctx); // nếu bạn có constructor nhận context
                var repo = new OrderRepository(dao); // nếu repo wrapper DAO
                var service = new OrderService(repo);

                // Nếu bạn dùng trực tiếp DAO:
                // var createdOrder = dao.CreateOrder(order, orderItems);

                // Gọi service để lưu
                service.CreateOrder(order, orderItems); // hoặc CreateOrder trả về Order

                MessageBox.Show($"Checkout thành công. OrderId = ", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public List<CartItem> GetCartItems() => _cart;
    }
}
