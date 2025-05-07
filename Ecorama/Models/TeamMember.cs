using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class TeamMember
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Role { get; set; }

    public string? ImageUrl { get; set; }

    public string? Email { get; set; }

    public string? GitHubLink { get; set; }

    public string? LinkedInLink { get; set; }
}
