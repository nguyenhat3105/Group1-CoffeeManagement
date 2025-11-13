using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CoffeeManagement.Helpers
{
    /// <summary>
    /// Chuyển string (ImgUrl) -> BitmapImage.
    /// Hỗ trợ:
    /// - null/empty -> fallback embedded resource "/Images/placeholder.png"
    /// - http(s) URL -> download via Uri
    /// - rooted local path C:\... -> load file
    /// - relative path starting with "/" assumed Resource in assembly (pack URI)
    /// - relative filename -> look in app's "Images" folder (exe base dir)
    /// </summary>
    public class ImagePathToBitmapConverter : IValueConverter
    {
        private const string DefaultFallback = "/Images/latte.jpg"; // đặt ảnh fallback trong project Resources

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? path = value as string;
            BitmapImage? bmp = null;

            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    bmp = LoadPackImage(DefaultFallback);
                    return bmp;
                }

                path = path.Trim();

                // web URL
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = uri;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }

                // absolute file path C:\...
                if (Path.IsPathRooted(path))
                {
                    if (File.Exists(path))
                    {
                        bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(path, UriKind.Absolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        return bmp;
                    }
                    else
                    {
                        return LoadPackImage(DefaultFallback);
                    }
                }

                // path starts with slash -> treat as pack resource (project resource)
                if (path.StartsWith("/"))
                {
                    return LoadPackImage(path);
                }

                // otherwise treat as relative filename inside ./Images folder of app
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string combined = Path.Combine(baseDir, "Images", path);
                if (File.Exists(combined))
                {
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(combined, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }

                // If nothing matched, fallback to DefaultFallback
                return LoadPackImage(DefaultFallback);
            }
            catch
            {
                // any failure -> fallback
                return LoadPackImage(DefaultFallback);
            }
        }

        private BitmapImage LoadPackImage(string packPath)
        {
            try
            {
                // Lấy tên assembly chứa converter (thường là CoffeeManagement)
                string asmName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                // Chuẩn hóa packPath
                string path = packPath.StartsWith("/") ? packPath : "/" + packPath;

                // Tạo pack URI với assembly; example:
                // pack://application:,,,/YourAssemblyName;component/Images/latte.jpg
                string uriString = $"pack://application:,,,/{asmName};component{path}";

                var uri = new Uri(uriString, UriKind.Absolute);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch (Exception ex)
            {
                // Optional: tạm log để debug (gỡ khi xong)
                System.Diagnostics.Debug.WriteLine($"LoadPackImage failed for '{packPath}': {ex}");
                return new BitmapImage();
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
