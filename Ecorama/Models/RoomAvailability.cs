using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class RoomAvailability
{
    public int AvailabilityId { get; set; }

    public int? RoomId { get; set; }

    public DateOnly? AvailableFromDate { get; set; }

    public DateOnly? AvailableToDate { get; set; }

    public TimeOnly? AvailableFromTime { get; set; }

    public TimeOnly? AvailableToTime { get; set; }

    public virtual Room? Room { get; set; }
}
