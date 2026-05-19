using System;
using System.Collections.Generic;

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

    public virtual Brand? Brand { get; set; }

    public virtual ProductType? ProductType { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
