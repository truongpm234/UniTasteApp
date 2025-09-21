using System;
using System.Collections.Generic;

namespace ReviewService.API.Models.Entity;

public partial class AppointmentInvite
{
    public int AppointmentInviteId { get; set; }

    public int? AppointmentId { get; set; }

    public int? UserId { get; set; }

    public bool? IsAccepted { get; set; }

    public DateTime? RespondedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }
}
