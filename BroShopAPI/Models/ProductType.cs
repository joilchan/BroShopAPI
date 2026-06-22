using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string Name { get; set; } = null!;

    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
