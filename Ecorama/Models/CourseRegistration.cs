using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class CourseRegistration
{
    public int Id { get; set; }

    public int? CourseId { get; set; }

    public int? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? RegisteredAt { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User? User { get; set; }
}
