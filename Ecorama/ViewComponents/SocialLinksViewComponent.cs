using Microsoft.AspNetCore.Mvc;
using Ecorama.Models;
using System.Linq;

public class SocialLinksViewComponent : ViewComponent
{
    private readonly MyDbContext _context;

    public SocialLinksViewComponent(MyDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var links = _context.SocialMediaLinks
            .Where(s => s.IsActive)
            .ToList();

        return View(links);
    }
}
