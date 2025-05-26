using System.Diagnostics;
using System.Linq;
using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class HomeController : Controller
    {

        private readonly MyDbContext _context;

        public HomeController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var sliders = _context.SliderItems.Where(s => s.IsActive).OrderBy(s => s.Order).ToList();
            var socialLinks = _context.SocialMediaLinks.Where(s => s.IsActive).ToList();


            HttpContext.Session.GetString("ProfileImagePath");


            var latestWorkshops = _context.Workshops
               .Where(w => w.IsActive)
               .OrderBy(w => w.Date)
               .Take(3)
               .ToList();



            // اجلب عدد الدروس لكل كورس في القائمة فقط
            var courses = _context.Courses
          .Where(c => c.IsActive)
          .OrderByDescending(c => c.CreatedAt)
          .Take(8)
          .ToList();

            // جلب عدد الدروس لكل كورس من جدول CourseLessons
            var courseIds = courses.Select(c => c.Id).ToList();

            var courseLessonsCounts = _context.CourseLessons
                .Where(cl => cl.CourseId.HasValue && courseIds.Contains(cl.CourseId.Value))
                .GroupBy(cl => cl.CourseId)
                .Select(g => new
                {
                    CourseId = (int?)g.Key,  // هنا تأكد أن المفتاح nullable int
                    LessonCount = g.Count()
                })
                .ToDictionary(x => x.CourseId, x => x.LessonCount);


            ViewBag.CourseLessonsCounts = courseLessonsCounts;



            var students = _context.Users.Where(s => s.Role == "User").Count();
            ViewBag.Students = students;

            var learnCourses = _context.Courses.Count();
            ViewBag.coursesLearn = learnCourses;

            var workShopsCount = _context.Workshops.Count();
            ViewBag.workshopsCount = workShopsCount;


            var latestNews = _context.News
               .Where(n => n.IsActive)
               .OrderByDescending(n => n.CreatedAt)
               .Take(3)
               .ToList();

            var partners = _context.Partners.ToList();



            var latestNewsTicker = _context.News
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .Take(4) 
                .ToList();

            ViewBag.LatestNewsTicker = latestNewsTicker;




            var viewModel = new HomeViewModel
            {
                Sliders = sliders,
                SocialLinks = socialLinks,
                LatestWorkshops = latestWorkshops,
                Courses = courses,
                LatestNews = latestNews,
                Partners = partners



            };

            return View(viewModel);
        }








        //  AboutUs



        public async Task<IActionResult> AboutUs()
        {
            var aboutUsList = await _context.AboutUs.ToListAsync();
            var teamMembersList = await _context.TeamMembers.ToListAsync();

            var viewModel = new TeamViewModel
            {
                AboutUs = aboutUsList,
                TeamMembers = teamMembersList
            };

            return View(viewModel);
        }






        // team 

        public async Task<IActionResult> Team()
        {
            var viewModel = new TeamViewModel
            {
                TeamMembers = await _context.TeamMembers.ToListAsync()
            };

            return View(viewModel);
        }









        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
