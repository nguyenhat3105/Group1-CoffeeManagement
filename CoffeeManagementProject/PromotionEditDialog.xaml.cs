using CoffeeManagement.DAL.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls; // <-- THÊM USING NÀY

namespace CoffeeManagement
{
    public partial class PromotionEditDialog : Window
    {
        public Promotion CurrentPromotion { get; private set; }
        private bool _isEditMode = false;

        public PromotionEditDialog(Promotion promotion = null)
        {
            InitializeComponent();

            if (promotion == null)
            {
                // Chế độ TẠO MỚI
                _isEditMode = false;
                Title = "Tạo Khuyến Mãi Mới";
                CurrentPromotion = new Promotion
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7),
                    IsActive = true,
                    // *** SỬA LỖI LOGIC: Mặc định là 2 (Tiền cố định) ***
                    DiscountType = 2
                };
            }
            else
            {
                // Chế độ CHỈNH SỬA
                _isEditMode = true;
                Title = $"Chỉnh Sửa: {promotion.Code}";
                CurrentPromotion = promotion;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCode.Text = CurrentPromotion.Code;
            TxtDescription.Text = CurrentPromotion.Description;
            TxtDiscountValue.Text = CurrentPromotion.DiscountValue.ToString("N0");
            TxtMinPurchaseAmount.Text = CurrentPromotion.MinPurchaseAmount.ToString("N0");
            TxtMaxDiscountAmount.Text = CurrentPromotion.MaxDiscountAmount?.ToString("N0");
            TxtUsageLimit.Text = CurrentPromotion.UsageLimit?.ToString();
            DpStartDate.SelectedDate = CurrentPromotion.StartDate;
            DpEndDate.SelectedDate = CurrentPromotion.EndDate;
            ChkIsActive.IsChecked = CurrentPromotion.IsActive;

            // *** THÊM LẠI: Logic load ComboBox dựa trên Tag ***
            foreach (ComboBoxItem item in CboDiscountType.Items)
            {
                if (item.Tag.ToString() == CurrentPromotion.DiscountType.ToString())
                {
                    CboDiscountType.SelectedItem = item;
                    break;
                }
            }

            if (_isEditMode)
            {
                TxtCode.IsEnabled = false;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Validate Dữ Liệu ---

            // *** THÊM LẠI: Validate ComboBox ***
            var selectedItem = CboDiscountType.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại giảm giá.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // (Tất cả validation khác của bạn giữ nguyên)
            if (string.IsNullOrWhiteSpace(TxtCode.Text))
            {
                MessageBox.Show("Mã code không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtCode.Focus();
                return;
            }

            if (!decimal.TryParse(TxtDiscountValue.Text, NumberStyles.Any, null, out decimal discountValue))
            {
                MessageBox.Show("Giá trị giảm giá không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtDiscountValue.Focus();
                return;
            }

            if (!decimal.TryParse(TxtMinPurchaseAmount.Text, NumberStyles.Any, null, out decimal minPurchase))
            {
                MessageBox.Show("Đơn tối thiểu không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtMinPurchaseAmount.Focus();
                return;
            }

            // (Validation cho MaxDiscount, UsageLimit, Dates giữ nguyên...)
            decimal? maxDiscount = null;
            if (!string.IsNullOrWhiteSpace(TxtMaxDiscountAmount.Text))
            {
                if (decimal.TryParse(TxtMaxDiscountAmount.Text, NumberStyles.Any, null, out decimal maxD))
                { maxDiscount = maxD; }
                else
                {
                    MessageBox.Show("Giảm tối đa không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtMaxDiscountAmount.Focus();
                    return;
                }
            }

            int? usageLimit = null;
            if (!string.IsNullOrWhiteSpace(TxtUsageLimit.Text))
            {
                if (int.TryParse(TxtUsageLimit.Text, out int limit))
                { usageLimit = limit; }
                else
                {
                    MessageBox.Show("Giới hạn lượt dùng không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtUsageLimit.Focus();
                    return;
                }
            }

            if (DpStartDate.SelectedDate == null || DpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Ngày bắt đầu và kết thúc không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DpEndDate.SelectedDate < DpStartDate.SelectedDate)
            {
                MessageBox.Show("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // --- 2. Gán dữ liệu hợp lệ vào CurrentPromotion ---
            CurrentPromotion.Code = TxtCode.Text.Trim().ToUpper();
            CurrentPromotion.Description = TxtDescription.Text.Trim();

            // *** SỬA LỖI CRASH: Lấy giá trị từ Tag của ComboBox ***
            CurrentPromotion.DiscountType = Convert.ToByte(selectedItem.Tag);

            CurrentPromotion.DiscountValue = discountValue;
            CurrentPromotion.MinPurchaseAmount = minPurchase;
            CurrentPromotion.MaxDiscountAmount = maxDiscount;
            CurrentPromotion.UsageLimit = usageLimit;
            CurrentPromotion.StartDate = DpStartDate.SelectedDate.Value;
            CurrentPromotion.EndDate = DpEndDate.SelectedDate.Value;
            CurrentPromotion.IsActive = ChkIsActive.IsChecked ?? false;

            // --- 3. Đóng Dialog và trả về 'true' ---
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}