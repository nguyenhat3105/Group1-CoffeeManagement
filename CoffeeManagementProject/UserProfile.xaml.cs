using CoffeeManagement.DAL.Models;
using CoffeeManagement.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : UserControl
    {
        // Lưu avatar trong AppData để khỏi gặp vấn đề quyền ghi khi cài vào Program Files
        private readonly string AvatarFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CoffeeManagement",
            "UserAvatars");

        // Kích thước tối đa chiều dài/chiều rộng của avatar khi lưu
        private const int AvatarMaxSize = 400;

        public UserProfile()
        {
            InitializeComponent();
            Loaded += UserProfile_Loaded;
        }

        private void UserProfile_Loaded(object? sender, RoutedEventArgs e)
        {
            LoadProfile();
            LoadAvatarOnStart();
        }

        private void LoadProfile()
        {
            var user = AppSession.CurrentUser;
            if (user == null)
            {
                TxtFullName.Text = "Guest";
                TxtEmail.Text = "";
                TxtUsername.Text = "";
                TxtCreatedAt.Text = "";
                TxtRole.Text = "";
                return;
            }

            TxtFullName.Text = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim();
            TxtEmail.Text = user.Email ?? "";
            TxtUsername.Text = user.Username ?? "";
            TxtCreatedAt.Text = user.CreatedAt.ToString("dd/MM/yyyy HH:mm");

            // load role name from DB (keep as you had)
            try
            {
                using var ctx = new CoffeeManagementDbContext();
                var role = ctx.Roles.FirstOrDefault(r => r.RoleId == user.RoleId);
                TxtRole.Text = role?.RoleName ?? "Unknown";
            }
            catch
            {
                TxtRole.Text = "Unknown";
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = AppSession.CurrentUser;
            var dialog = new UserUpdateProfile(user);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                // Refresh profile display
                LoadProfile();
            }
        }

        // ---------- Avatar logic ----------

        // Load avatar when control loaded (if file exists)
        private void LoadAvatarOnStart()
        {
            try
            {
                var user = AppSession.CurrentUser;
                if (user == null) return;

                string path = GetAvatarPathForUser(user.Id);
                if (File.Exists(path))
                {
                    SetImageSourceFromFile(path);
                    return;
                }

                // If not found by exact name, try to find any file starting with user_{id} (legacy)
                if (Directory.Exists(AvatarFolder))
                {
                    var candidates = Directory.GetFiles(AvatarFolder, $"user_{user.Id}*")
                                              .Where(f => IsImageFile(f))
                                              .OrderByDescending(f => File.GetLastWriteTime(f))
                                              .ToList();
                    if (candidates.Any())
                    {
                        SetImageSourceFromFile(candidates.First());
                    }
                }
            }
            catch
            {
                // ignore load errors
            }
        }

        // Button click handler - open file dialog, copy and display
        private void BtnUpdateAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Ảnh (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            bool? result = dlg.ShowDialog();
            if (result != true) return;

            string srcPath = dlg.FileName;
            if (!File.Exists(srcPath)) return;

            var user = AppSession.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Không tìm thấy người dùng hiện tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // ensure folder exists
                if (!Directory.Exists(AvatarFolder))
                    Directory.CreateDirectory(AvatarFolder);

                // create dest path user_{id}.ext (overwrite previous)
                string ext = Path.GetExtension(srcPath).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                string destPath = GetAvatarPathForUser(user.Id);

                // Save resized copy to destPath (reduces file size)
                SaveResizedCopy(srcPath, destPath, AvatarMaxSize);

                // Load into Image control (safe load)
                SetImageSourceFromFile(destPath);

                // Optionally: persist path to local settings (Properties.Settings) — omitted per request
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật ảnh:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Builds path like ...\AppData\Roaming\CoffeeManagement\UserAvatars\user_123.jpg
        private string GetAvatarPathForUser(int userId)
        {
            // try preserve jpg extension; if pre-existing different ext, above LoadAvatarOnStart will find it
            string fileName = $"user_{userId}.jpg";
            return Path.Combine(AvatarFolder, fileName);
        }

        // Check extension
        private bool IsImageFile(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp";
        }

        // Load image into AvatarImage safely
        private void SetImageSourceFromFile(string path)
        {
            // defensive
            if (!File.Exists(path)) return;

            var bitmap = new BitmapImage();
            // open stream to avoid file lock
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // important to close stream after load
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            AvatarImage.Source = bitmap;
        }

        // Save resized copy (maintain aspect ratio), using JPEG encoder
        private void SaveResizedCopy(string sourcePath, string destPath, int maxSize)
        {
            // Load original
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(sourcePath);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            int originalW = bmp.PixelWidth;
            int originalH = bmp.PixelHeight;

            double scale = 1.0;
            if (originalW > maxSize || originalH > maxSize)
            {
                scale = (double)maxSize / Math.Max(originalW, originalH);
            }

            int newW = (int)Math.Round(originalW * scale);
            int newH = (int)Math.Round(originalH * scale);

            // Create TransformedBitmap for scaling
            var tb = new TransformedBitmap(bmp, new ScaleTransform(scale, scale));

            // Encode to JPEG
            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 85;
            encoder.Frames.Add(BitmapFrame.Create(tb));

            // Ensure destination directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(destPath) ?? AvatarFolder);

            using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }
    }
}
