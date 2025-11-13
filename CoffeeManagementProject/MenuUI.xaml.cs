using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeManagement
{
    public partial class MenuUI : UserControl
    {
        private readonly MenuItemsService _menuService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        private readonly PromotionService _promotionService;

        private List<DAL.Models.MenuItem> _allMenuItems = new List<DAL.Models.MenuItem>();
        private List<Category> _allCategories = new List<Category>();

        private User _selectedCustomer = null;
        private Promotion _appliedPromotion = null;

        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();

        private int _selectedCategoryId = 0;
        private string _searchQuery = "";

        public MenuUI()
        {
            InitializeComponent();

            var ctx = new CoffeeManagementDbContext();
            _menuService = new MenuItemsService(new MenuItemsRepository(new MenuItemsDAO(ctx)));
            _orderService = new OrderService(new OrderRepository(new OrderDAO(ctx)));

            // *** SỬA LỖI COMMENT (Code của bạn đã đúng) ***
            _promotionService = new PromotionService(new PromotionRepository(new PromotionDAO(ctx))); // Khởi tạo PromotionService
            _userService = new UserService(new UserRepository(new UserDao(ctx)));

            // Gán DataContext và sources
            this.DataContext = this;
            CartItemsControl.ItemsSource = CartItems;

            CartItems.CollectionChanged += (s, e) =>
            {
                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            };

            LoadInitialData();
            CheckUserRole();
        }

        // =============================================
        // HÀM KIỂM TRA QUYỀN HẠN
        // =============================================
        private void CheckUserRole()
        {
            // RoleId 2 = Staff
            if (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 2)
            {
                StaffCustomerSearchPanel.Visibility = Visibility.Visible;
                TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
            }
            else
            {
                StaffCustomerSearchPanel.Visibility = Visibility.Collapsed;
            }
        }

        // =============================================
        // HÀM TÌM KHÁCH HÀNG
        // =============================================
        private void BtnFindCustomer_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtCustomerSearch.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                _selectedCustomer = null;
                TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
                return;
            }

            try
            {
                var foundUser = _userService.GetByUsername(query);

                if (foundUser != null && foundUser.RoleId == 3) // Role 3 là Customer
                {
                    _selectedCustomer = foundUser;
                    TxtSelectedCustomerInfo.Text = $"Đang chọn: {foundUser.FullName} ({foundUser.Username})";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Green;
                }
                else
                {
                    _selectedCustomer = null;
                    TxtSelectedCustomerInfo.Text = "Không tìm thấy khách hàng thành viên.";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                _selectedCustomer = null;
                TxtSelectedCustomerInfo.Text = "Lỗi khi tìm kiếm.";
                TxtSelectedCustomerInfo.Foreground = Brushes.Red;
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        // =============================================
        // HÀM TẢI DỮ LIỆU
        // =============================================
        private void LoadInitialData()
        {
            try
            {
                _allMenuItems = _menuService.GetAll().Where(m => m.IsAvailable == true).ToList();
                _allCategories = _menuService.GetAllCategories().ToList();
                PopulateCategoryFilter();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =============================================
        // HÀM TẠO BỘ LỌC RADIOBUTTON (Khớp với XAML)
        // =============================================
        private void PopulateCategoryFilter()
        {
            CategoryFilterPanel.Children.Clear();

            var allButton = new RadioButton
            {
                Content = "Tất cả",
                IsChecked = true,
                Style = (Style)FindResource("CategoryButtonStyle"),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = 0
            };
            allButton.Click += CategoryFilter_Click;
            CategoryFilterPanel.Children.Add(allButton);

            foreach (var category in _allCategories)
            {
                var btn = new RadioButton
                {
                    Content = category.Name,
                    Style = (Style)FindResource("CategoryButtonStyle"),
                    Margin = new Thickness(0, 0, 8, 0),
                    Tag = category.Id
                };
                btn.Click += CategoryFilter_Click;
                CategoryFilterPanel.Children.Add(btn);
            }
        }

        // =============================================
        // HÀM LỌC
        // =============================================
        private void ApplyFilters()
        {
            var filteredList = _allMenuItems
                .Where(item =>
                    (_selectedCategoryId == 0 || item.CategoryId == _selectedCategoryId) &&
                    (string.IsNullOrEmpty(_searchQuery) || (item.Name?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0))
                )
                .ToList();

            MenuItemsControl.ItemsSource = filteredList;
        }

        // =============================================
        // HÀM SỰ KIỆN CHO BỘ LỌC (Khớp với XAML)
        // =============================================
        private void CategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is int categoryId)
            {
                _selectedCategoryId = categoryId;
                ApplyFilters();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = TxtSearch.Text ?? "";
            ApplyFilters();
        }

        // =============================================
        // CÁC HÀM CÒN LẠI (GIỎ HÀNG, THANH TOÁN, v.v.)
        // =============================================

        #region Các hàm Giỏ hàng, Thanh toán, Xử lý ảnh (Giữ nguyên)

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DAL.Models.MenuItem? selectedItem = null;
                var fe = sender as FrameworkElement;
                if (fe != null)
                {
                    if (fe.Tag is DAL.Models.MenuItem miTag)
                        selectedItem = miTag;
                    else if (fe.DataContext is DAL.Models.MenuItem miCtx)
                        selectedItem = miCtx;
                }

                if (selectedItem == null) return;

                var existingItem = CartItems.FirstOrDefault(i => i.Item.Id == selectedItem.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartItems.Add(new CartItem { Item = selectedItem, Quantity = 1 });
                }

                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIncreaseItem_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            CartItem? cartItem = null;
            if (fe != null)
            {
                if (fe.Tag is CartItem tagItem) cartItem = tagItem;
                else if (fe.DataContext is CartItem ctxItem) cartItem = ctxItem;
            }

            if (cartItem == null) return;

            cartItem.Quantity++;
            CartItemsControl.Items.Refresh();
            UpdateCartTotal();
        }

        private void BtnDecreaseItem_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            CartItem? cartItem = null;
            if (fe != null)
            {
                if (fe.Tag is CartItem tagItem) cartItem = tagItem;
                else if (fe.DataContext is CartItem ctxItem) cartItem = ctxItem;
            }

            if (cartItem == null) return;

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                CartItems.Remove(cartItem);
            }

            CartItemsControl.Items.Refresh();
            UpdateCartTotal();
        }

        private void UpdateCartTotal()
        {
            decimal subtotal = CartItems.Sum(item => item.Quantity * (item.Item?.Price ?? 0));
            decimal discountAmount = 0;

            if (_appliedPromotion != null)
            {
                if (_appliedPromotion.DiscountType == 1) // percent
                {
                    discountAmount = subtotal * (_appliedPromotion.DiscountValue / 100m);
                    if (_appliedPromotion.MaxDiscountAmount.HasValue && discountAmount > _appliedPromotion.MaxDiscountAmount.Value)
                        discountAmount = _appliedPromotion.MaxDiscountAmount.Value;
                }
                else if (_appliedPromotion.DiscountType == 2) // fixed
                {
                    discountAmount = _appliedPromotion.DiscountValue;
                }
                if (discountAmount > subtotal) discountAmount = subtotal;
            }

            decimal total = subtotal - discountAmount;

            TxtCartSubtotal.Text = $"{subtotal:N0}đ";
            TxtCartDiscount.Text = discountAmount > 0 ? $"-{discountAmount:N0}đ" : "0đ";
            TxtCartTotal.Text = $"{total:N0}đ";
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Giỏ hàng trống. Vui lòng chọn món trước khi đặt hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? customerIdForOrder = null;
            int? staffIdForOrder = null;
            bool isStaffOrdering = (AppSession.CurrentUser != null && AppSession.CurrentUser.RoleId == 2);

            if (isStaffOrdering)
            {
                staffIdForOrder = AppSession.CurrentUser.Id;
                customerIdForOrder = _selectedCustomer?.Id;
            }
            else if (AppSession.CurrentUser != null)
            {
                staffIdForOrder = null;
                customerIdForOrder = AppSession.CurrentUser.Id;
            }
            else
            {
                MessageBox.Show("Vui lòng đăng nhập để hoàn tất đơn hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal subtotal = CartItems.Sum(i => i.Quantity * (i.Item?.Price ?? 0));
            decimal discountAmount = 0;
            int? promotionId = null;

            if (_appliedPromotion != null)
            {
                if (_appliedPromotion.DiscountType == 1)
                {
                    discountAmount = subtotal * (_appliedPromotion.DiscountValue / 100);
                    if (_appliedPromotion.MaxDiscountAmount.HasValue && discountAmount > _appliedPromotion.MaxDiscountAmount.Value)
                    {
                        discountAmount = _appliedPromotion.MaxDiscountAmount.Value;
                    }
                }
                else if (_appliedPromotion.DiscountType == 2)
                {
                    discountAmount = _appliedPromotion.DiscountValue;
                }
                if (discountAmount > subtotal) discountAmount = subtotal;

                promotionId = _appliedPromotion.Id;
            }

            var newOrder = new Order
            {
                CustomerId = customerIdForOrder,
                StaffId = staffIdForOrder,
                CreatedAt = DateTime.Now,
                Status = 0, // 0 = Pending
                IsPaid = false,
                Note = TxtOrderNote.Text.Trim(),
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TotalAmount = subtotal - discountAmount,
                PromotionId = promotionId,

                OrderItems = CartItems.Select(ci => new OrderItem
                {
                    MenuItemId = ci.Item.Id,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Item.Price
                }).ToList()
            };

            try
            {
                _orderService.CreateOrder(newOrder, newOrder.OrderItems.ToList());

                if (_appliedPromotion != null)
                {
                    _promotionService.IncrementUsage(_appliedPromotion.Id);
                }

                MessageBox.Show($"Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                TxtOrderNote.Text = string.Empty;

                _appliedPromotion = null;
                TxtVoucherCode.Text = string.Empty;
                TxtVoucherInfo.Text = string.Empty;

                if (isStaffOrdering)
                {
                    _selectedCustomer = null;
                    TxtCustomerSearch.Text = string.Empty;
                    TxtSelectedCustomerInfo.Text = "Đang chọn: Khách lẻ (Vãng lai)";
                    TxtSelectedCustomerInfo.Foreground = Brushes.Gray;
                }

                CartItemsControl.Items.Refresh();
                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                var full = new System.Text.StringBuilder();
                Exception? cur = ex;
                while (cur != null)
                {
                    full.AppendLine(cur.GetType().FullName + ": " + cur.Message);
                    full.AppendLine(cur.StackTrace ?? "");
                    full.AppendLine("----");
                    cur = cur.InnerException;
                }

                System.Diagnostics.Debug.WriteLine("[Order Error] " + full.ToString());
                MessageBox.Show($"Có lỗi xảy ra khi đặt hàng:\n{ex.Message}\n\n(Check Output window for stack trace)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyVoucher_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtVoucherCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                _appliedPromotion = null;
                TxtVoucherInfo.Text = "";
                UpdateCartTotal();
                return;
            }

            decimal currentSubtotal = CartItems.Sum(item => item.Quantity * (item.Item?.Price ?? 0));

            var promo = _promotionService.GetValidPromotionByCode(code, currentSubtotal);

            if (promo == null)
            {
                _appliedPromotion = null;
                TxtVoucherInfo.Text = "Mã không hợp lệ hoặc không đủ điều kiện.";
                TxtVoucherInfo.Foreground = Brushes.Red;
            }
            else
            {
                _appliedPromotion = promo;
                TxtVoucherInfo.Text = $"Áp dụng thành công: {promo.Description}";
                TxtVoucherInfo.Foreground = Brushes.Green;
            }

            UpdateCartTotal(); // Tính toán lại tổng tiền
        }

        private void MenuItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Image img) return;

            string? imgPath = null;
            if (img.Tag is string tagStr) imgPath = tagStr;
            else if (img.DataContext != null)
            {
                var dc = img.DataContext;
                var prop = dc.GetType().GetProperty("ImgUrl")
                         ?? dc.GetType().GetProperty("ImagePath")
                         ?? dc.GetType().GetProperty("Image")
                         ?? dc.GetType().GetProperty("Url");
                if (prop != null) imgPath = prop.GetValue(dc)?.ToString();
            }

            System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] raw imgPath='{imgPath}' for DataContext={img.DataContext?.GetType().Name}");

            string asmName = Assembly.GetExecutingAssembly().GetName().Name ?? "CoffeeManagement";
            string fallbackPack = $"pack://application:,,,/{asmName};component/Images/latte.jpg";

            void SetFallback()
            {
                System.Diagnostics.Debug.WriteLine("[MenuItemImage_Loaded] setting fallback image");
                try { img.Source = new BitmapImage(new Uri(fallbackPack, UriKind.Absolute)); }
                catch { img.Source = null; }
            }

            bool TrySetBitmap(Uri uri)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = uri;
                    bmp.EndInit();
                    bmp.Freeze();
                    img.Source = bmp;
                    System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] loaded Uri: {uri}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MenuItemImage_Loaded] load Uri failed: {uri} -> {ex.Message}");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(imgPath))
            {
                SetFallback();
                return;
            }

            imgPath = imgPath.Trim();

            if (imgPath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
            {
                if (TrySetBitmap(new Uri(imgPath, UriKind.Absolute))) return;
            }

            if (imgPath.StartsWith("/")) imgPath = imgPath.TrimStart('/');

            string[] tryExtensions = new[] { "", ".jpg", ".jpeg", ".png", ".gif" };
            string baseName = imgPath;
            string ext = Path.GetExtension(imgPath);
            if (!string.IsNullOrEmpty(ext))
            {
                baseName = imgPath.Substring(0, imgPath.Length - ext.Length);
                tryExtensions = new[] { ext, ".jpg", ".jpeg", ".png", ".gif" };
            }

            if (Uri.TryCreate(imgPath, UriKind.Absolute, out Uri? absoluteUri))
            {
                if (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeFile)
                {
                    if (TrySetBitmap(absoluteUri)) return;
                }
            }

            if (Path.IsPathRooted(imgPath))
            {
                if (File.Exists(imgPath))
                {
                    if (TrySetBitmap(new Uri(imgPath, UriKind.Absolute))) return;
                }
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory();
            var fileCandidates = new List<string>();
            foreach (var ext2 in tryExtensions.Select(te => baseName + te))
            {
                fileCandidates.Add(Path.Combine(baseDir, ext2));
                fileCandidates.Add(Path.Combine(baseDir, "Images", ext2));
                fileCandidates.Add(Path.Combine(baseDir, "images", ext2));
                fileCandidates.Add(Path.Combine(baseDir, "Resources", ext2));
            }

            foreach (var cand in fileCandidates.Distinct())
            {
                try
                {
                    if (File.Exists(cand))
                    {
                        if (TrySetBitmap(new Uri(cand, UriKind.Absolute))) return;
                    }
                }
                catch { /* ignore */ }
            }

            var packCandidates = new List<string>();
            foreach (var ext2 in tryExtensions.Select(te => baseName + te))
            {
                packCandidates.Add($"pack://application:,,,/{asmName};component/Images/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/images/{ext2}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/{ext2.ToLowerInvariant()}");
                packCandidates.Add($"pack://application:,,,/{asmName};component/Images/{ext2.ToLowerInvariant()}");
            }

            foreach (var p in packCandidates.Distinct())
            {
                try
                {
                    if (TrySetBitmap(new Uri(p, UriKind.Absolute))) return;
                }
                catch { }
            }

            SetFallback();
        }

        #endregion
    }
}