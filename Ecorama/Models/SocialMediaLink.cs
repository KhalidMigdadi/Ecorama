using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class SocialMediaLink
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? IconClass { get; set; }

    public string Url { get; set; } = null!;

    public string? IconColor { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
