using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class Comment
{
    public int CommentId { get; set; }

    public int PostId { get; set; }

    public int? ParentId { get; set; }

    public int AuthorUserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int ReactionsCount { get; set; }

    public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();

    public virtual ICollection<Comment> InverseParent { get; set; } = new List<Comment>();

    public virtual Comment? Parent { get; set; }

    public virtual Post Post { get; set; } = null!;
}
