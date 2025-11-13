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
    /// Interaction logic for UserUpdateProfile.xaml
    /// </summary>
    public partial class UserUpdateProfile : Window
    {
        private readonly User _user;
        public bool IsSaved { get; private set; }

        public UserUpdateProfile(User user)
        {
            InitializeComponent();
            _user = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            TxtUsername.Text = _user.Username;
            TxtEmail.Text = _user.Email;
            TxtFirstName.Text = _user.FirstName;
            TxtLastName.Text = _user.LastName;
            TxtPassword.Password = _user.Password;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all required fields!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var ctx = new CoffeeManagementDbContext();
            var dao = new UserDao(ctx);
            var repo = new UserRepository(dao);
            var service = new UserService(repo);

            try
            {
                _user.Username = username;  
                _user.Email = email;
                _user.FirstName = firstName;
                _user.LastName = lastName;
                _user.Password = password;
                _user.UpdatedAt = DateTime.UtcNow;

                service.Update(_user);

                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                IsSaved = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
