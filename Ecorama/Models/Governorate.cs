using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Governorate
{
    public int GovernorateId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
