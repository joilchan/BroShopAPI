using System;
using System.Collections.Generic;

namespace BroShopAPI.Models;

public partial class ProductVariant
{
    public int ProductVariantId { get; set; }

    public int ProductId { get; set; }

    public string Size { get; set; } = null!;

    public int StockQuantity { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Product? Product { get; set; } = null!;
}
