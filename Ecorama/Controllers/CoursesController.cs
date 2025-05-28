using ClosedXML.Excel;
using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class CoursesController : Controller
    {

        private readonly MyDbContext _context;
        public CoursesController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        public IActionResult Create()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course, IFormFile imageFile, IFormFile pdfFile)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (ModelState.IsValid)
            {
                // مسارات التخزين
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // رفع الصورة
                if (imageFile != null && imageFile.Length > 0)
                {
                    string imagePath = Path.Combine("uploads/images", Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
                    string fullImagePath = Path.Combine(wwwRootPath, imagePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath)!);
                    using (var stream = new FileStream(fullImagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    course.ImageUrl = "/" + imagePath.Replace("\\", "/");
                }

                // رفع ملف PDF
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    string pdfPath = Path.Combine("uploads/pdfs", Guid.NewGuid() + Path.GetExtension(pdfFile.FileName));
                    string fullPdfPath = Path.Combine(wwwRootPath, pdfPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPdfPath)!);
                    using (var stream = new FileStream(fullPdfPath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(stream);
                    }
                    course.PdfUrl = "/" + pdfPath.Replace("\\", "/");
                }

                course.CreatedAt = DateTime.Now;
                course.IsActive = true;

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(course);
        }


        public async Task<IActionResult> Edit(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var course = await _context.Courses.FindAsync(id);
            return course == null ? NotFound() : View(course);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Course course, IFormFile? imageFile, IFormFile? pdfFile)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (ModelState.IsValid)
            {
                var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == course.Id);
                if (existingCourse == null) return NotFound();

                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // رفع صورة جديدة
                if (imageFile != null && imageFile.Length > 0)
                {
                    string imagePath = Path.Combine("uploads/images", Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
                    string fullImagePath = Path.Combine(wwwRootPath, imagePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath)!);
                    using (var stream = new FileStream(fullImagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    course.ImageUrl = "/" + imagePath.Replace("\\", "/");
                }
                else
                {
                    course.ImageUrl = existingCourse.ImageUrl;
                }

                // رفع PDF جديد
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    string pdfPath = Path.Combine("uploads/pdfs", Guid.NewGuid() + Path.GetExtension(pdfFile.FileName));
                    string fullPdfPath = Path.Combine(wwwRootPath, pdfPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPdfPath)!);
                    using (var stream = new FileStream(fullPdfPath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(stream);
                    }
                    course.PdfUrl = "/" + pdfPath.Replace("\\", "/");
                }
                else
                {
                    course.PdfUrl = existingCourse.PdfUrl;
                }

                course.CreatedAt = existingCourse.CreatedAt; 
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }


        public async Task<IActionResult> Delete(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ToggleActivation(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");


            }
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsActive = !course.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }






        public async Task<IActionResult> ExportCoursesToExcel()
        {
            var courses = await _context.Courses
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Courses");

            // رؤوس الأعمدة
            worksheet.Cell(1, 1).Value = "العنوان";
            worksheet.Cell(1, 2).Value = "الوصف";
            worksheet.Cell(1, 3).Value = "رابط PDF";
            worksheet.Cell(1, 4).Value = "تاريخ الإضافة";
            worksheet.Cell(1, 5).Value = "تاريخ الدورة";
            worksheet.Cell(1, 6).Value = "الحالة";

            // تنسيق الرؤوس
            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var course in courses)
            {
                worksheet.Cell(row, 1).Value = course.Title ?? "-";
                worksheet.Cell(row, 2).Value = course.Description ?? "-";
                worksheet.Cell(row, 3).Value = course.PdfUrl ?? "-";
                worksheet.Cell(row, 4).Value = course.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
                worksheet.Cell(row, 5).Value = course.Date?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cell(row, 6).Value = course.IsActive ? "نشط" : "غير نشط";

                row++;
            }

            // جعل الأعمدة تأخذ حجم المحتوى
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Courses.xlsx");
        }


    }
}

