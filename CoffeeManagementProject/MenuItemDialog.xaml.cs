using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CoffeeManagement
{
    public partial class MenuItemDialog : Window
    {
        private readonly MenuItem? _existingItem;
        public bool IsSaved { get; private set; }

        public MenuItemDialog(MenuItem? menuItem = null)
        {
            InitializeComponent();
            _existingItem = menuItem;
            LoadCategories();
            if (_existingItem != null)
            {
                LoadExistingData();
            }
        }

        private void LoadCategories()
        {
            using var ctx = new CoffeeManagementDbContext();
            var categories = ctx.Categories.ToList();
            CboCategory.ItemsSource = categories;
            CboCategory.DisplayMemberPath = "Name";
            CboCategory.SelectedValuePath = "Id";
        }

        private void LoadExistingData()
        {
            TxtName.Text = _existingItem.Name;
            TxtDescription.Text = _existingItem.Description;
            TxtPrice.Text = _existingItem.Price.ToString("0.##");
            TxtImageUrl.Text = _existingItem.ImgUrl;
            ChkAvailable.IsChecked = _existingItem.IsAvailable;
            CboCategory.SelectedValue = _existingItem.CategoryId;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtPrice.Text) ||
                CboCategory.SelectedValue == null)
            {
                MessageBox.Show("Please fill in required fields!", "Warning",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal price))
            {
                MessageBox.Show("Invalid price format!", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using var ctx = new CoffeeManagementDbContext();
            var dao = new MenuItemsDAO(ctx);
            var repo = new MenuItemsRepository(dao);
            var service = new MenuItemsService(repo);

            try
            {
                if (_existingItem == null)
                {
                    // ADD
                    var newItem = new MenuItem
                    {
                        Name = TxtName.Text.Trim(),
                        Description = TxtDescription.Text.Trim(),
                        Price = price,
                        CategoryId = (int)CboCategory.SelectedValue,
                        ImgUrl = TxtImageUrl.Text.Trim(),
                        IsAvailable = ChkAvailable.IsChecked ?? false
                    };
                    service.Create(newItem);
                    MessageBox.Show("Menu item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // UPDATE
                    _existingItem.Name = TxtName.Text.Trim();
                    _existingItem.Description = TxtDescription.Text.Trim();
                    _existingItem.Price = price;
                    _existingItem.CategoryId = (int)CboCategory.SelectedValue;
                    _existingItem.ImgUrl = TxtImageUrl.Text.Trim();
                    _existingItem.IsAvailable = ChkAvailable.IsChecked ?? false;

                    service.Update(_existingItem);
                    MessageBox.Show("Menu item updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                IsSaved = true;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
