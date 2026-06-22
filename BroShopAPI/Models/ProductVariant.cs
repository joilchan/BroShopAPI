using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class ProductVariant
{
    public int ProductVariantId { get; set; }

    public int ProductId { get; set; }

    public string Size { get; set; } = null!;

    public int StockQuantity { get; set; }

    [JsonIgnore]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [JsonIgnore]
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    [JsonIgnore]
    public virtual Product? Product { get; set; } = null!;
}
