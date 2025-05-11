using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Village
{
    public int VillageId { get; set; }

    public int DistrictId { get; set; }

    public string Name { get; set; } = null!;

    public virtual District District { get; set; } = null!;
}
