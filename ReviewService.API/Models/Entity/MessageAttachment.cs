using System;
using System.Collections.Generic;

namespace ReviewService.API.Models.Entity;

public partial class MessageAttachment
{
    public int MessageAttachmentId { get; set; }

    public int? MessageId { get; set; }

    public string? FileUrl { get; set; }

    public string? FileType { get; set; }

    public virtual Message? Message { get; set; }
}
