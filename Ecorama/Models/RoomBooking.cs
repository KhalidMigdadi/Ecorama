using System;
using System.Collections.Generic;

namespace Ecorama.Models;

public partial class RoomBooking
{
    public int BookingId { get; set; }

    public int? RoomId { get; set; }

    public int? UserId { get; set; }

    public DateOnly? BookingDate { get; set; }

    public TimeOnly? BookingFrom { get; set; }

    public TimeOnly? BookingTo { get; set; }

    public int? NumberOfGuests { get; set; }

    public string? Purpose { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Room? Room { get; set; }

    public virtual User? User { get; set; }
}
