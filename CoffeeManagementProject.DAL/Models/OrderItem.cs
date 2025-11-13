using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeManagementProject.DAL.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual MenuItem MenuItem { get; set; } = null!;

    [NotMapped] // Đảm bảo Entity Framework không cố gắng map thuộc tính này vào DB
    public decimal TotalPrice
    {
        get
        {
            // Tính toán Thành tiền: Số lượng * Giá đơn vị (lấy từ MenuItem)
            return Quantity * (MenuItem?.Price ?? 0);
        }
    }

    public virtual Order Order { get; set; } = null!;
}
