using System;
using System.Globalization;
using System.Windows.Data;

namespace CoffeeManagement.Helpers
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Không xác định";

            int status = System.Convert.ToInt32(value);

            return status switch
            {
                0 => "Chờ xử lý",
                1 => "Đang pha chế",
                2 => "Hoàn tất",
                3 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Không cần convert ngược lại
            throw new NotImplementedException();
        }
    }
}
