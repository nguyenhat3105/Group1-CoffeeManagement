using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System.Windows;
using System.Windows.Controls;
using System; // *** THÊM MỚI ***
using System.Collections.Generic; // *** THÊM MỚI ***
using System.Linq; // *** THÊM MỚI ***
using CoffeeManagement.Helpers; // *** THÊM MỚI (Để lấy AppSession) ***

namespace CoffeeManagement
{
    public partial class AdminPromotionsView : UserControl
    {
        private PromotionService _promotionService;
        private CoffeeManagementDbContext context;
        private List<Promotion> _allPromotions;

        public AdminPromotionsView()
        {
            InitializeComponent();

            // *** THÊM MỚI: Gọi hàm kiểm tra Role ***
            CheckUserRole();

            try
            {
                context = new CoffeeManagementDbContext();
                _promotionService = new PromotionService(new PromotionRepository(new PromotionDAO(context)));
                LoadPromotions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo service: {ex.Message}", "Lỗi nghiêm trọng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // *** THÊM MỚI: Hàm kiểm tra Role ***
        private void CheckUserRole()
        {
            // Giả sử RoleId 2 là Staff (Nhân viên)
            if (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 2)
            {
                // Ẩn nút "Tạo Khuyến Mãi Mới"
                BtnAddPromotion.Visibility = Visibility.Collapsed;

                // Các nút Sửa/Xóa bên trong DataTemplate sẽ tự động ẩn
                // vì chúng ta đã bind Visibility của chúng vào BtnAddPromotion trong XAML
            }
            // (Không cần 'else', vì mặc định là 'Visible')
        }


        /// <summary>
        /// Tải (hoặc tải lại) danh sách khuyến mãi từ service
        /// </summary>
        private void LoadPromotions()
        {
            try
            {
                _allPromotions = _promotionService.GetAllPromotion();
                PromotionsItemsControl.ItemsSource = _allPromotions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng gõ vào ô tìm kiếm.
        /// </summary>
        private void TxtSearchPromotion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allPromotions == null) return;
            string searchText = TxtSearchPromotion.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                PromotionsItemsControl.ItemsSource = _allPromotions;
                return;
            }

            var filteredList = _allPromotions.Where(p =>
                p.Code.ToLower().Contains(searchText) ||
                (p.Description != null && p.Description.ToLower().Contains(searchText))
            ).ToList();

            PromotionsItemsControl.ItemsSource = filteredList;
        }

        /// <summary>
        /// Mở cửa sổ/dialog để TẠO khuyến mãi mới.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PromotionEditDialog();
            if (dialog.ShowDialog() == true)
            {
                var newPromotion = dialog.CurrentPromotion;
                try
                {
                    _promotionService.AddPromotion(newPromotion);
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Mở cửa sổ/dialog để SỬA khuyến mãi đã chọn.
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var promotionToEdit = button.Tag as Promotion;
            if (promotionToEdit == null) return;

            var dialog = new PromotionEditDialog(promotionToEdit);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _promotionService.UpdatePromotion(promotionToEdit);
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật khuyến mãi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Xử lý nút Xóa
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var promotionToDelete = button.Tag as Promotion;
            if (promotionToDelete == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa khuyến mãi: {promotionToDelete.Code}?",
                                        "Xác nhận xóa",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _promotionService.DeletePromotion(promotionToDelete.Id);
                    LoadPromotions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}