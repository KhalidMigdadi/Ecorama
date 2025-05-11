using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class District
{
    public int DistrictId { get; set; }

    public int GovernorateId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Governorate Governorate { get; set; } = null!;

    public virtual ICollection<Village> Villages { get; set; } = new List<Village>();
}
