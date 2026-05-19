using System;
using System.Collections.Generic;

namespace BroShopAPI.Models;

public partial class OrderProduct
{
    public int ProductVariantId { get; set; }

    public int OrderId { get; set; }

    public int Quantity { get; set; }

    public decimal? PriceAtPurchase { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
