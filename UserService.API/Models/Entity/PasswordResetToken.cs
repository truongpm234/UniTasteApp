using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }

    public int? UserId { get; set; }

    public string? Token { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool? IsUsed { get; set; }

    public virtual User? User { get; set; }
}
