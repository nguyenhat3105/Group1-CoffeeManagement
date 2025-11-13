using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
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
using System.Windows.Shapes;

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class UserDialog : Window
    {
        private readonly User? _existingUser;  // null = Add, not null = Edit
        public bool IsSaved { get; private set; } = false;

        public UserDialog(User? user = null)
        {
            InitializeComponent();
            _existingUser = user;

            if (_existingUser != null)
            {
                // Edit mode
                TxtTitle.Text = "Edit User";
                TxtEmail.Text = _existingUser.Email;
                TxtUsername.Text = _existingUser.Username;
                TxtFirstName.Text = _existingUser.FirstName;
                TxtLastName.Text = _existingUser.LastName;
                TxtPassword.Password = _existingUser.Password;

                foreach (ComboBoxItem item in CboRole.Items)
                {
                    if (item.Tag.ToString() == _existingUser.RoleId.ToString())
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
            else
            {
                TxtTitle.Text = "Add New User";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password.Trim();
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();
            var selectedRole = (CboRole.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(selectedRole))
            {
                MessageBox.Show("Please fill in all fields!", "Warning",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!byte.TryParse(selectedRole, out byte roleId))
            {
                MessageBox.Show("Invalid role selected!", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using var ctx = new CoffeeManagementDbContext();
            var dao = new UserDao(ctx);
            var repo = new UserRepository(dao);
            var service = new UserService(repo);

            try
            {
                if (_existingUser == null)
                {
                    // Add new user
                    var newUser = new User
                    {
                        Email = email,
                        Username = username,
                        Password = password,
                        FirstName = firstName,
                        LastName = lastName,
                        RoleId = roleId,
                        CreatedAt = DateTime.UtcNow
                    };

                    service.Create(newUser);
                    MessageBox.Show("User added successfully!", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing
                    _existingUser.Email = email;
                    _existingUser.Username = username;
                    _existingUser.Password = password;
                    _existingUser.FirstName = firstName;
                    _existingUser.LastName = lastName;
                    _existingUser.RoleId = roleId;
                    _existingUser.UpdatedAt = DateTime.UtcNow;

                    repo.Update(_existingUser);
                    MessageBox.Show("User updated successfully!", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

                IsSaved = true;
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
            this.Close();
        }
    }
}
