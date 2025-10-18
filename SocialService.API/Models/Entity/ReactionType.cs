using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class ReactionType
{
    public int ReactionTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();

    public virtual ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
}
