using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
