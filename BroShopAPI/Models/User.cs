using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    [JsonIgnore]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;
}
