using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class AboutU
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
