using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class PostShare
{
    public int PostShareId { get; set; }

    public int OriginalPostId { get; set; }

    public int SharerUserId { get; set; }

    public string? ShareComment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post OriginalPost { get; set; } = null!;
}
