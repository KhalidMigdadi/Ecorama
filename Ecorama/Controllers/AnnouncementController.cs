using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly MyDbContext _context;

        public AnnouncementController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var announcements = _context.Announcements.OrderByDescending(a => a.CreatedAt).ToList();
            return View(announcements);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.Now;
                _context.Announcements.Add(announcement);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(announcement);
        }

        public IActionResult Edit(int id)
        {
            var announcement = _context.Announcements.Find(id);
            if (announcement == null)
                return NotFound();

            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                _context.Announcements.Update(announcement);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(announcement);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var announcement = _context.Announcements.Find(id);
            if (announcement == null)
                return NotFound();

            _context.Announcements.Remove(announcement);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
