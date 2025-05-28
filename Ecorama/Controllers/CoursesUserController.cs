using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class CoursesUserController : Controller
    {
        private readonly MyDbContext _context;

        public CoursesUserController(MyDbContext context)
        {
            _context = context;
        }

        // عرض الكورسات الفعالة فقط
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Where(c => c.IsActive)
                .ToListAsync();

            List<int> registeredCourseIds = new List<int>();

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                registeredCourseIds = await _context.CourseRegistrations
                    .Where(r => r.UserId == userId.Value && r.CourseId != null)
                    .Select(r => r.CourseId.Value)
                    .ToListAsync();
            }

            ViewBag.RegisteredCourseIds = registeredCourseIds;

            return View(courses);
        }


        public IActionResult Register(int courseId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            // جلب بيانات المستخدم من قاعدة البيانات
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
                return NotFound("المستخدم غير موجود.");

            // تجهيز نموذج التسجيل مع تعبئة البيانات
            var registration = new CourseRegistration
            {
                CourseId = courseId,
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.MiddleName} {user.LastName}".Trim(),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(registration);
        }



        [HttpPost]
        public async Task<IActionResult> Register(CourseRegistration registration)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            // تأكيد عدم التسجيل المكرر
            bool alreadyRegistered = await _context.CourseRegistrations
                .AnyAsync(r => r.CourseId == registration.CourseId && r.UserId == userId);

            if (alreadyRegistered)
            {
                TempData["Message"] = "أنت مشترك بالفعل في هذا الكورس.";
                return RedirectToAction("MyCourses");
            }

            registration.UserId = userId.Value;
            registration.RegisteredAt = DateTime.Now;

            _context.CourseRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "تم التسجيل في الكورس بنجاح!";
            return RedirectToAction("MyCourses");
        }




        // عرض كورسات المستخدم المسجل فيها
        public async Task<IActionResult> MyCourses()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            var myCourses = await _context.CourseRegistrations
                .Include(r => r.Course)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(myCourses);
        }

        // عرض دروس الكورس المسجل فيه
        public async Task<IActionResult> Lessons(int courseId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            bool isRegistered = await _context.CourseRegistrations
                .AnyAsync(r => r.CourseId == courseId && r.UserId == userId);

            if (!isRegistered)
                return RedirectToAction("Index");

            var lessons = await _context.CourseLessons
                .Where(l => l.CourseId == courseId)
                .ToListAsync();

            ViewBag.Course = await _context.Courses.FindAsync(courseId);
            return View(lessons);
        }
    }
}
