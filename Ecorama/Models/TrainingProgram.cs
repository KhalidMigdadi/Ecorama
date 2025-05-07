using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class TrainingProgram
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public DateOnly? Date { get; set; }

    public string? FormFields { get; set; }

    public virtual ICollection<TrainingProgramRegistration> TrainingProgramRegistrations { get; set; } = new List<TrainingProgramRegistration>();

    public virtual ICollection<UserCourseSubscription> UserCourseSubscriptions { get; set; } = new List<UserCourseSubscription>();
}
