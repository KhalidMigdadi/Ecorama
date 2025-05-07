using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Course
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public string? PdfUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CourseLesson> CourseLessons { get; set; } = new List<CourseLesson>();

    public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();

    public virtual ICollection<UserCourseSubscription> UserCourseSubscriptions { get; set; } = new List<UserCourseSubscription>();
}
