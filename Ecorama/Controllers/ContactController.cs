using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class ContactController : Controller
    {
        private readonly MyDbContext _context;

        public ContactController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForm(ContactU contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.Now;
                _context.ContactUs.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage1212"] = "تم إرسال رسالتك بنجاح! سنتواصل معك قريبًا.";
                return RedirectToAction("Index");
            }

            return View("Index", contact);
        }
    }
}
