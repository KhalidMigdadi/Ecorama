using ClosedXML.Excel;
using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;


namespace Ecorama.Controllers
{
    public class NewsController : Controller
    {

        private readonly MyDbContext _context;

        public NewsController(MyDbContext context)
        {
            _context = context;
        }


        public IActionResult News()
        {
            var news = _context.News.Where(n => n.IsActive == true).ToList();

            if (news.Count > 0)
            {
                return View(news);
            }
            else
            {
                ViewBag.news = "empty";
                return View(news); 
            }
        }



        public IActionResult NewsDetails(int id)
        {
            var New = _context.News.Find(id);

            if (New == null)
            {
                ViewBag.empty = true;
                return View();
            }

            return View(New);
        }



        public async Task<IActionResult> Advertisements()
        {
            // جلب البيانات من قاعدة البيانات
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }


        public IActionResult AdvertisementsDetails(int id)
        {
            // الحصول على الإعلان المحدد
            var announcement = _context.Announcements
                .FirstOrDefault(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            // الحصول على الإعلانات المشابهة (نفس الفئة أو الأحدث)
            var relatedAnnouncements = _context.Announcements
                .Where(a => a.Id != id) // استبعاد الإعلان الحالي
                .OrderByDescending(a => a.CreatedAt)
                .Take(4) // أخذ 4 إعلانات مشابهة
                .ToList();

            // تمرير الإعلانات المشابهة عبر ViewBag
            ViewBag.RelatedAnnouncements = relatedAnnouncements;

            return View(announcement);
        }



     

    }
}

