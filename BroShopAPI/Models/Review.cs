using System;
using System.Collections.Generic;

namespace BroShopAPI.Models;

public partial class Review
{
    public int ProductId { get; set; }

    public int UserId { get; set; }

    public string Text { get; set; } = null!;

    public decimal Rating { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
