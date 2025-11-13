using System;
using System.Collections.Generic;

namespace CoffeeManagementProject.DAL.Models;

public partial class MenuItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public string? ImgUrl { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
