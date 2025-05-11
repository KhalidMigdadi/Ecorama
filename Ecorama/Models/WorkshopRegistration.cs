using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class WorkshopRegistration
{
    public int Id { get; set; }

    public int? WorkshopId { get; set; }

    public int? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Organization { get; set; }

    public string? Notes { get; set; }

    public DateTime? RegisteredAt { get; set; }

    public virtual User? User { get; set; }

    public virtual Workshop? Workshop { get; set; }
}
