using System.Diagnostics;
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



            var viewModel = new HomeViewModel
            {
                Sliders = sliders,
                SocialLinks = socialLinks
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
