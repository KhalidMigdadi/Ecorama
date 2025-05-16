using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class courseLessonscontroller : Controller
    {
        private readonly MyDbContext _context;
        public courseLessonscontroller(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            var lessons = _context.CourseLessons
                .Where(l => l.CourseId == courseId)
                .ToList();
            ViewBag.CourseId = courseId;
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
        public async Task<IActionResult> Create(CourseLesson lesson)
        {
            if (ModelState.IsValid)
            {
                _context.CourseLessons.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { courseId = lesson.CourseId });
            }
            return View(lesson);
        }



        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.CourseLessons.FindAsync(id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CourseLesson lesson)
        {
            if (ModelState.IsValid)
            {
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
                //int courseId = lesson.CourseId;
                int courseId = lesson.CourseId ?? 0;
                _context.CourseLessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { courseId });
            }
            return NotFound();
        }

    }
}
