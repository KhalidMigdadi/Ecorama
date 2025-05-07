using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class CourseLesson
{
    public int Id { get; set; }

    public int? CourseId { get; set; }

    public string? Title { get; set; }

    public string? ImageUrl { get; set; }

    public string? YoutubeLink { get; set; }

    public virtual Course? Course { get; set; }
}
