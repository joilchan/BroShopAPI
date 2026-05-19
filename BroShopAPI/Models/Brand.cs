using System;
using System.Collections.Generic;

namespace BroShopAPI.Models;

public partial class Brand
{
    public int BrandId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
