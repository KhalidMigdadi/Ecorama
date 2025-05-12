using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class ContactU
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Subject { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
