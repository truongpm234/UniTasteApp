using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class AiLog
{
    public int AiLogId { get; set; }

    public int? UserId { get; set; }

    public string? RequestType { get; set; }

    public string? InputText { get; set; }

    public string? OutputText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
