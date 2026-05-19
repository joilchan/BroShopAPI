using System;
using System.Collections.Generic;

namespace BroShopAPI.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string? Address { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal Amount { get; set; }

    public decimal? Discount { get; set; }

    public string? PromoCode { get; set; }

    public decimal DeliveryCost { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual User User { get; set; } = null!;
}
