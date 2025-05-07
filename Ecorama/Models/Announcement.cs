using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Announcement
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }
}
