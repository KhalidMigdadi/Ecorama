using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class SliderItem
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public int Order { get; set; }

    public string? ImageFilePath { get; set; }
}
