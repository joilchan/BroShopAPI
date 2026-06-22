using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class Cart
{
    public int UserId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;

    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
