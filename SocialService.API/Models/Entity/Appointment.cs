using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? GroupId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<AppointmentInvite> AppointmentInvites { get; set; } = new List<AppointmentInvite>();

    public virtual Group? Group { get; set; }
}
