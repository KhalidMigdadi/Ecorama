using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class TrainingProgramRegistration
{
    public int Id { get; set; }

    public int? TrainingProgramId { get; set; }

    public int? UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ExperienceLevel { get; set; }

    public string? MotivationText { get; set; }

    public DateTime? RegisteredAt { get; set; }

    public virtual TrainingProgram? TrainingProgram { get; set; }

    public virtual User? User { get; set; }
}
