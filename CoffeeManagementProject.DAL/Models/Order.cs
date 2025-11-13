using System;
using System.Collections.Generic;

namespace CoffeeManagementProject.DAL.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? StaffId { get; set; }

    public byte Status { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Note { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public int? PromotionId { get; set; }

    public virtual User? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Promotion? Promotion { get; set; }

    public virtual User? Staff { get; set; }
}
