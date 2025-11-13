using System;
using System.Collections.Generic;

namespace CoffeeManagementProject.DAL.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public byte DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinPurchaseAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int CurrentUsage { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
