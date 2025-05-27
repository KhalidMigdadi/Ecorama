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
        public IActionResult Create(Announcement announcement, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // تحديد مجلد الحفظ
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // إنشاء اسم فريد للملف
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // حفظ الملف فعلياً في المجلد
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(fileStream);
                    }

                    // حفظ اسم الملف (أو المسار النسبي) في الـ Announcement
                    announcement.ImageUrl = uniqueFileName;
                }

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
        public IActionResult Edit(Announcement announcement, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Announcements.Find(announcement.Id);
                if (existing == null)
                    return NotFound();

                existing.Title = announcement.Title;
                existing.Content = announcement.Content;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // حفظ الصورة الجديدة
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    // حذف الصورة القديمة (اختياري)
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        var oldPath = Path.Combine(uploadsFolder, existing.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // تحديث اسم الصورة
                    existing.ImageUrl = uniqueFileName;
                }

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
