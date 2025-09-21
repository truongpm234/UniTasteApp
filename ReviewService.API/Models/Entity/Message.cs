using System;
using System.Collections.Generic;

namespace ReviewService.API.Models.Entity;

public partial class Message
{
    public int MessageId { get; set; }

    public int? SenderId { get; set; }

    public int? GroupId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<MessageAttachment> MessageAttachments { get; set; } = new List<MessageAttachment>();
}
