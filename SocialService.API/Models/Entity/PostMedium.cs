using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class PostMedium
{
    public int PostMediaId { get; set; }

    public int PostId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
