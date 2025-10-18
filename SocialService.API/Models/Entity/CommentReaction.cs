using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class CommentReaction
{
    public int CommentReactionId { get; set; }

    public int CommentId { get; set; }

    public int UserId { get; set; }

    public int ReactionTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Comment Comment { get; set; } = null!;

    public virtual ReactionType ReactionType { get; set; } = null!;
}
