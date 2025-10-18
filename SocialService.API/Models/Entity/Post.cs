using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class Post
{
    public int PostId { get; set; }

    public int AuthorUserId { get; set; }

    public string? Title { get; set; }

    public string Content { get; set; } = null!;

    public byte? Rating { get; set; }

    public bool IsReview { get; set; }

    public string Visibility { get; set; } = null!;

    public int? OriginalPostId { get; set; }

    public string? ShareComment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int ReactionsCount { get; set; }

    public int CommentsCount { get; set; }

    public int SharesCount { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<PostMedium> PostMedia { get; set; } = new List<PostMedium>();

    public virtual ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();

    public virtual ICollection<PostRestaurantTag> PostRestaurantTags { get; set; } = new List<PostRestaurantTag>();

    public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
