// File: OrderDetailDialog.xaml.cs (ĐÃ SỬA)
using CoffeeManagement.DAL.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class OrderDetailDialog : Window
    {
        public OrderDetailDialog(Order order)
        {
            InitializeComponent();

            this.DataContext = order;

            TxtOrderId.Text = $"Chi Tiết Đơn Hàng #{order.Id}";
            TxtOrderDate.Text = $"Ngày tạo: {order.CreatedAt:dd/MM/yyyy HH:mm}";

            //TxtCustomerName.Text = order.Customer != null ?
            //    order.Customer.FullName : $"Khách hàng ID: {order.CustomerId}";

            //TxtOrderNote.Text = "Không có ghi chú"; // Giả định

            ItemsList.ItemsSource = order.OrderItems;

            TxtSubTotal.Text = $"{order.TotalAmount:N0}đ";



            ItemsList.ItemsSource = order.OrderItems.Select(i => new
            {
                MenuItem = i.MenuItem,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}