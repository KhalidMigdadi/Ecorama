using Ecorama.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            return View();
        }




        public IActionResult ViewAllUsers()
        {
            return View(_context.Users.ToList());
        }





        [HttpPost]
        public IActionResult ToggleActivation(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // تغيير حالة المستخدم بين تفعيل وتعطيل
            user.IsActive = !user.IsActive;
            _context.SaveChanges();


            return RedirectToAction("ViewAllUsers");
        }



        public IActionResult seeAllWorkShop()
        {

            var allWorkShops = _context.Workshops.ToList();

            var latestWorkshops = allWorkShops
                .Where(w => w.Date != null)
                .OrderByDescending(w => w.Date)
                .Take(15);

            return View(latestWorkshops);
        }




        public IActionResult AddNewWorkshop()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewWorkshop(Workshop workshop)
        {
            if (ModelState.IsValid)
            {
                _context.Workshops.Add(workshop);
                _context.SaveChanges();
                return RedirectToAction("seeAllWorkShop");
            }
            return View(workshop);
        }









        [HttpPost]
        public IActionResult DeleteWorkshop(int id)
        {
            var workshop = _context.Workshops.FirstOrDefault(w => w.Id == id);
            if (workshop == null)
            {
                return NotFound();
            }

            _context.Workshops.Remove(workshop);
            _context.SaveChanges();
            return RedirectToAction("seeAllWorkShop");
        }




        public IActionResult EditWorkShop(int id)
        {
            var currentWorkshop = _context.Workshops.Find(id);
            return View(currentWorkshop);
        }




        [HttpPost]
        public IActionResult EditWorkShop(Workshop workshop)
        {
            if (!ModelState.IsValid)
                return View("EditWorkShop");

            _context.Workshops.Update(workshop);
            _context.SaveChanges();

            return RedirectToAction("seeAllWorkShop");
        }



    }




}


