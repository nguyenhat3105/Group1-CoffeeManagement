using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class UsersListControl : UserControl
    {
        private List<User> _allUsers = new();

        public UsersListControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using var context = new CoffeeManagementDbContext();
            var userDAO = new UserDao(context);
            var userRepo = new UserRepository(userDAO);
            var userService = new UserService(userRepo);

            _allUsers = userService.GetAll().ToList();
            DgUsers.ItemsSource = _allUsers;
        }

        // 📦 Tìm kiếm realtime khi người dùng gõ
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        // 📦 Khi bấm nút 🔍
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            string keyword = TxtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                DgUsers.ItemsSource = _allUsers;
                return;
            }

            var filtered = _allUsers.Where(u =>
                (u.Username != null && u.Username.ToLower().Contains(keyword)) ||
                (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(keyword)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(keyword))
            ).ToList();

            DgUsers.ItemsSource = filtered;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UserDialog();
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();

            if (dlg.IsSaved)
            {
                LoadUsers();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is User selectedUser)
            {
                var dlg = new UserDialog(selectedUser);
                dlg.Owner = Window.GetWindow(this);
                dlg.ShowDialog();

                if (dlg.IsSaved)
                {
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn người dùng để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is User u)
            {
                var res = MessageBox.Show($"Xóa người dùng {u.Username} ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    using var ctx = new CoffeeManagementDbContext();
                    var dao = new UserDao(ctx);
                    var repo = new UserRepository(dao);
                    repo.Delete(u.Id);
                    LoadUsers();
                }
            }
        }
    }
}
