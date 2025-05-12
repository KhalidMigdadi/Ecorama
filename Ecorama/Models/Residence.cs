using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class Residence
{
    public int ResidenceId { get; set; }

    public int UserId { get; set; }

    public string Governorate { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Village { get; set; } = null!;

    public bool? IsCustomVillage { get; set; }

    public int? GovernorateId { get; set; }

    public int? DistrictId { get; set; }

    public int? VillageId { get; set; }

    public virtual District? DistrictNavigation { get; set; }

    public virtual Governorate? GovernorateNavigation { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Village? VillageNavigation { get; set; }
}
