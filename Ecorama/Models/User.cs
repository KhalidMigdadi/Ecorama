using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();

    public virtual ICollection<TrainingProgramRegistration> TrainingProgramRegistrations { get; set; } = new List<TrainingProgramRegistration>();

    public virtual ICollection<UserCourseSubscription> UserCourseSubscriptions { get; set; } = new List<UserCourseSubscription>();
}
