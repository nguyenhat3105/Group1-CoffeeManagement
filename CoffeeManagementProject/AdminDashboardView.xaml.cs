using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.DAL.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Diagnostics;

namespace CoffeeManagement
{
    public partial class AdminDashboardView : UserControl
    {
        private readonly IOrderService orderService =
            new OrderService(new OrderRepository(new OrderDAO(new CoffeeManagementDbContext())));

        public AdminDashboardView()
        {
            InitializeComponent();
            Loaded += AdminDashboardView_Loaded;
        }

        private void AdminDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải Dashboard: {ex.Message}\n\n(Bạn có chắc đã Include(OrderItems, MenuItem) chưa?)",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardData()
        {
            var allOrders = orderService.GetAllOrders();

            // Chỉ tính các đơn hoàn thành
            var orders = allOrders
                .Where(o => o.Status == 1 && o.IsPaid == true)
                .ToList();

            if (!orders.Any())
            {
                TxtTotalOrders.Text = "0";
                TxtTotalRevenue.Text = "0₫";
                TxtTotalItems.Text = "0";
                RevenueChart.Series.Clear();
                RevenueChart.AxisX[0].Labels = null;
                TopItemsChart.Series.Clear();
                return;
            }

            // --- Tổng quan ---
            TxtTotalOrders.Text = orders.Count.ToString();

            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            Debug.Write(totalRevenue);
            TxtTotalRevenue.Text = $"{totalRevenue:N0}₫";

            int totalItems = orders.Sum(o => o.OrderItems?.Sum(i => i.Quantity) ?? 0);
            TxtTotalItems.Text = totalItems.ToString();

            // --- Nạp dữ liệu biểu đồ ---
            LoadRevenueChart(orders);
            LoadTopItemsChart(orders);
        }

        // =======================================================
        // Biểu đồ doanh thu (Line Chart)
        // =======================================================
        private void LoadRevenueChart(List<Order> orders)
        {
            // Đảm bảo CreatedAt đúng giờ Việt Nam và nhóm theo ngày thực tế
            var byDate = orders
                .ToList()
                .GroupBy(o => o.CreatedAt.ToLocalTime().Date) // tránh lệch múi giờ
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.Date)
                .Take(7) // 7 ngày gần nhất
                .OrderBy(x => x.Date)
                .ToList();

            var revenueValues = new ChartValues<decimal>(byDate.Select(x => x.Revenue));
            var dateLabels = byDate.Select(x => x.Date.ToString("dd/MM")).ToList();

            RevenueChart.Series.Clear();
            RevenueChart.Series.Add(new LineSeries
            {
                Title = "Doanh thu",
                Values = revenueValues,
                Stroke = (Brush)FindResource("RevenueBrush"),
                Fill = Brushes.Transparent,
                DataLabels = true,
                LabelPoint = chartPoint => $"{chartPoint.Y:N0}₫"
            });

            RevenueChart.AxisX[0].Labels = dateLabels;
            RevenueChart.AxisY[0].LabelFormatter = value => $"{value:N0}₫";
        }

        // =======================================================
        // Biểu đồ Top 5 sản phẩm (Pie Chart)
        // =======================================================
        private void LoadTopItemsChart(List<Order> orders)
        {
            // Yêu cầu Include(OrderItems.MenuItem)
            var topItems = orders
                .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
                .Where(oi => oi.MenuItem != null)
                .GroupBy(oi => oi.MenuItem.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Total = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var seriesCollection = new SeriesCollection();

            foreach (var item in topItems)
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = item.Name,
                    Values = new ChartValues<int> { item.Total },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Participation:P0}"
                });
            }

            TopItemsChart.Series = seriesCollection;
        }
    }
}
