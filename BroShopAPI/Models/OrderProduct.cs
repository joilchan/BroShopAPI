using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class OrderProduct
{
    public int ProductVariantId { get; set; }

    public int OrderId { get; set; }

    public int Quantity { get; set; }

    public decimal? PriceAtPurchase { get; set; }

    [JsonIgnore]
    public virtual Order Order { get; set; } = null!;

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
