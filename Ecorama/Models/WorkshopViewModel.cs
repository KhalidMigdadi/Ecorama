namespace Ecorama.Models
{
    public class WorkshopViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string WebSiteUrl { get; set; }
        public bool IsActive { get; set; }
        public int Duration { get; set; }
        public int SeatsAvailable { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
