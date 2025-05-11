using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Language
{
    public int LanguageId { get; set; }

    public int UserId { get; set; }

    public string LanguageName { get; set; } = null!;

    public string? CustomLanguageName { get; set; }

    public string ProficiencyLevel { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
