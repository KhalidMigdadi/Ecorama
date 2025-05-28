using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Workshop
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public string? ImageUrl { get; set; }

    public string? WebSiteUrl { get; set; }

    public bool IsActive { get; set; }

    public int? Duration { get; set; }

    public int? SeatsAvailable { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Organization { get; set; }

    public virtual ICollection<UserCourseSubscription> UserCourseSubscriptions { get; set; } = new List<UserCourseSubscription>();

    public virtual ICollection<WorkshopRegistration> WorkshopRegistrations { get; set; } = new List<WorkshopRegistration>();
}
