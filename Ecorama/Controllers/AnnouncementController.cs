using ClosedXML.Excel;
using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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



        public async Task<IActionResult> ExportAnnouncementsToExcel()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("الإعلانات");

                // إعداد العناوين
                worksheet.Cell(1, 1).Value = "الرقم";
                worksheet.Cell(1, 2).Value = "العنوان";
                worksheet.Cell(1, 3).Value = "المحتوى";
                worksheet.Cell(1, 4).Value = "تاريخ الإنشاء";

                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                for (int i = 0; i < announcements.Count; i++)
                {
                    var item = announcements[i];

                    worksheet.Cell(row, 1).Value = i + 1;
                    worksheet.Cell(row, 2).Value = item.Title ?? "-";
                    worksheet.Cell(row, 3).Value = item.Content ?? "-";
                    worksheet.Cell(row, 4).Value = item.CreatedAt?.ToString("yyyy/MM/dd HH:mm") ?? "-";

                    worksheet.Row(row).Style.Alignment.WrapText = true; // التفاف النص داخل الخلية
                    row++;
                }

                worksheet.Columns().AdjustToContents(); // ضبط حجم الأعمدة تلقائياً حسب المحتوى
                worksheet.Rows().AdjustToContents(); // ضبط ارتفاع الصفوف حسب المحتوى

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "الإعلانات.xlsx");
                }
            }
        }


    }
}
