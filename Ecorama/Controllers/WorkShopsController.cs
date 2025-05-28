using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class WorkShopsController : Controller
    {
        private readonly MyDbContext _context;

        public WorkShopsController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Workshops()
        {
            var workshops = _context.Workshops
                                    .Where(w => w.IsActive);

            var worksShops = workshops.ToList();

            return View(worksShops);
        }



        public IActionResult WorkshopsDetails(int id)
        {
            var workshop = _context.Workshops.FirstOrDefault(w => w.Id == id && w.IsActive);
            if (workshop == null)
                return NotFound();

            return View(workshop);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForWorkshop(int workshopId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");  
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var workshop = await _context.Workshops.FindAsync(workshopId);
            if (workshop == null)
            {
                TempData["ErrorMessage"] = "الورشة غير موجودة.";
                return RedirectToAction("Workshops");
            }

            // تحقق إذا المستخدم مسجل مسبقاً في نفس الورشة لمنع التسجيل المكرر
            bool alreadyRegistered = await _context.WorkshopRegistrations
                .AnyAsync(wr => wr.UserId == userId && wr.WorkshopId == workshopId);

            if (alreadyRegistered)
            {
                TempData["ErrorMessage"] = "أنت مسجل بالفعل في هذه الورشة.";
                return RedirectToAction("WorkshopsDetails", new { id = workshopId });
            }

            var registration = new WorkshopRegistration
            {
                WorkshopId = workshopId,
                UserId = userId,
                FullName = $"{user.FirstName} {user.MiddleName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Organization = workshop.Organization,
                Notes = $"الورشة: {workshop.Title}",
                RegisteredAt = DateTime.Now
            };

            _context.WorkshopRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم التسجيل بنجاح!";
            return RedirectToAction("WorkshopsDetails", new { id = workshopId });
        }

    }
}
