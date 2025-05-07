using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Partner
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? ImageUrl { get; set; }

    public string? WebsiteUrl { get; set; }
}
