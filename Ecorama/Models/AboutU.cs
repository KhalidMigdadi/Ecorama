using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class AboutU
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImagePath { get; set; }
}
