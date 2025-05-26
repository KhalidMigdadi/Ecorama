using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class WorkShopsController : Controller
    {
        private readonly MyDbContext _context;

        public WorkShopsController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Workshops()
        {
            var workshops = _context.Workshops
                                    .Where(w => w.IsActive);

            var worksShops = workshops.ToList();

            return View(worksShops);
        }



        public IActionResult WorkshopsDetails(int id)
        {
            var workshop = _context.Workshops.FirstOrDefault(w => w.Id == id && w.IsActive);
            if (workshop == null)
                return NotFound();

            return View(workshop);
        }


    }
}
