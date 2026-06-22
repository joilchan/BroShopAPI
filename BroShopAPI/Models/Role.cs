using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BroShopAPI.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
