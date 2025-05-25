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
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Course course, IFormFile imageFile, IFormFile pdfFile)
        {
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
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(course);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            return course == null ? NotFound() : View(course);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Course course, IFormFile? imageFile, IFormFile? pdfFile)
        {
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

                course.CreatedAt = existingCourse.CreatedAt; // نحتفظ بتاريخ الإنشاء الأصلي
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }


        public async Task<IActionResult> Delete(int id)
        {
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
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsActive = !course.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


    }
}

