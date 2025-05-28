namespace Ecorama.Models
{
    public class WorkshopRegistrationViewModel
    {
        public int WorkshopId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Organization { get; set; }
        public string? Notes { get; set; }
    }
}
