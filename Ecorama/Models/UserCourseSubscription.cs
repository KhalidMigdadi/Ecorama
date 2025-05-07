using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class UserCourseSubscription
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? CourseId { get; set; }

    public int? TrainingProgramId { get; set; }

    public int? WorkshopId { get; set; }

    public DateTime? SubscribedAt { get; set; }

    public virtual Course? Course { get; set; }

    public virtual TrainingProgram? TrainingProgram { get; set; }

    public virtual User? User { get; set; }

    public virtual Workshop? Workshop { get; set; }
}
