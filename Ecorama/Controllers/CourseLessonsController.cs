using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class CourseLessonsController : Controller
    {
        private readonly MyDbContext _context;
        public CourseLessonsController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            var lessons = await _context.CourseLessons
                .Where(l => l.CourseId == courseId)
                .ToListAsync();

            ViewBag.CourseId = courseId;
            var course = await _context.Courses.FindAsync(courseId);
            ViewBag.CourseTitle = course?.Title;

            return View(lessons);
        }

        public IActionResult Create(int courseId)
        {
            var lesson = new CourseLesson
            {
                CourseId = courseId
            };
            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseLesson lesson, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // رفع الصورة إذا تم رفعها
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lesson-images");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    lesson.ImageUrl = "/uploads/lesson-images/" + uniqueFileName;
                }

                _context.CourseLessons.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { courseId = lesson.CourseId });
            }

            return View(lesson);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.CourseLessons.FindAsync(id);
            return lesson == null ? NotFound() : View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CourseLesson lesson, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingLesson = await _context.CourseLessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lesson.Id);
                if (existingLesson == null) return NotFound();

                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // إذا تم رفع صورة جديدة
                if (imageFile != null && imageFile.Length > 0)
                {
                    string imagePath = Path.Combine("uploads/lesson-images", Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
                    string fullImagePath = Path.Combine(wwwRootPath, imagePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath)!);
                    using (var stream = new FileStream(fullImagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    lesson.ImageUrl = "/" + imagePath.Replace("\\", "/");
                }
                else
                {
                    // إذا ما رفع صورة جديدة، نحتفظ بالقديمة
                    lesson.ImageUrl = existingLesson.ImageUrl;
                }

                _context.CourseLessons.Update(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { courseId = lesson.CourseId });
            }

            return View(lesson);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.CourseLessons.FindAsync(id);
            if (lesson != null)
            {
                int courseId = lesson.CourseId ?? 0;
                _context.CourseLessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { courseId });
            }
            return NotFound();
        }
    }
}
