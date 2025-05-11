using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Education
{
    public int EducationId { get; set; }

    public int UserId { get; set; }

    public string EducationLevel { get; set; } = null!;

    public string ProgramType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
