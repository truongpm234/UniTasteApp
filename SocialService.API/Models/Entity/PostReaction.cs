using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class PostReaction
{
    public int PostReactionId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int ReactionTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual ReactionType ReactionType { get; set; } = null!;
}
