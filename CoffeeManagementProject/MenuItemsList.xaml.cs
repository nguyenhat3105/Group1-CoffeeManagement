using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for MenuItemsList.xaml
    /// </summary>
    public partial class MenuItemsList : UserControl
    {
        public MenuItemsList()
        {
            InitializeComponent();
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            using var context = new CoffeeManagementDbContext();
            var dao = new MenuItemsDAO(context);
            var repo = new MenuItemsRepository(dao);
            var service = new MenuItemsService(repo);

            var list = service.GetAll().ToList();
            GridMenuItems.ItemsSource = list;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new MenuItemDialog(); // Add mode
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();

            if (dlg.IsSaved)
            {
                LoadMenuItems();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (GridMenuItems.SelectedItem is DAL.Models.MenuItem selected)
            {
                var dlg = new MenuItemDialog(selected);
                dlg.Owner = Window.GetWindow(this);
                dlg.ShowDialog();

                if (dlg.IsSaved)
                {
                    LoadMenuItems();
                }
            }
            else
            {
                MessageBox.Show("Please select a menu item to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridMenuItems.SelectedItem is DAL.Models.MenuItem selected)
            {
                var res = MessageBox.Show($"Delete menu item '{selected.Name}' ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    using var ctx = new CoffeeManagementDbContext();
                    var dao = new MenuItemsDAO(ctx);
                    var repo = new MenuItemsRepository(dao);
                    repo.Delete(selected.Id);
                    LoadMenuItems();
                }
            }
            else
            {
                MessageBox.Show("Please select a menu item to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
