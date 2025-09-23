using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public int? RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<AiLog> AiLogs { get; set; } = new List<AiLog>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<UserAccessory> UserAccessories { get; set; } = new List<UserAccessory>();

    public virtual ICollection<UserPremium> UserPremia { get; set; } = new List<UserPremium>();
}
