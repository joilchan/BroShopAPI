using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? BrandId { get; set; }

    public int? ProductTypeId { get; set; }

    public int? Discount { get; set; }

    [JsonIgnore]
    public virtual Brand? Brand { get; set; }

    [JsonIgnore]
    public virtual ProductType? ProductType { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
