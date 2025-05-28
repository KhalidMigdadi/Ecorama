using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Ecorama.Controllers
{
    public class SocialController : Controller
    {
        private readonly MyDbContext _context;
        public SocialController(MyDbContext context)
        {
            _context = context;
        }

        // Index - عرض قائمة الروابط
        public async Task<IActionResult> Index()
        {
            var socialLinks = await _context.SocialMediaLinks.ToListAsync();
            return View(socialLinks);
        }










        // جلب روابط الشبكات الاجتماعية
        public async Task<IActionResult> GetSocialLinks()
        {
            var socialLinks = await _context.SocialMediaLinks
                                           .Where(link => link.IsActive)
                                           .ToListAsync();
            return PartialView("_SocialLinks", socialLinks);
        }

        // Social Media 
        public IActionResult CreateSocial()
        {
            var socialMediaList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Facebook", Value = "Facebook" },
                new SelectListItem { Text = "X", Value = "X" },
                new SelectListItem { Text = "Instagram", Value = "Instagram" },
                new SelectListItem { Text = "LinkedIn", Value = "LinkedIn" },
                new SelectListItem { Text = "GitHub", Value = "GitHub" },
                new SelectListItem { Text = "YouTube", Value = "YouTube" },
            };

            ViewBag.SocialMediaOptions = socialMediaList;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSocial(SocialMediaLink link)
        {
            if (ModelState.IsValid)
            {
                link.CreatedAt = DateTime.Now;
                _context.Add(link);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // لإعادة القائمة في حال فشل التحقق
            ViewBag.SocialMediaOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Facebook", Value = "Facebook" },
                    new SelectListItem { Text = "X", Value = "X" },
                    new SelectListItem { Text = "Instagram", Value = "Instagram" },
                    new SelectListItem { Text = "LinkedIn", Value = "LinkedIn" },
                    new SelectListItem { Text = "GitHub", Value = "GitHub" },
                    new SelectListItem { Text = "YouTube", Value = "YouTube" },
                };

            return View(link);
        }








        // صفحة تعديل رابط سوشيال ميديا
        public async Task<IActionResult> EditSocial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var socialLink = await _context.SocialMediaLinks.FindAsync(id);
            if (socialLink == null)
            {
                return NotFound();
            }
            return View(socialLink);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSocial(int id, SocialMediaLink link)
        {
            if (id != link.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // تحديد اللون والأيقونة بناءً على اسم الشبكة الاجتماعية
                    switch (link.Name)
                    {
                        case "Facebook":
                            link.IconColor = "#3b5998";
                            link.IconClass = "fab fa-facebook-f";
                            break;
                        case "X":
                            link.IconColor = "#000000";
                            link.IconClass = "fab fa-x-twitter";
                            break;
                        case "Google":
                            link.IconColor = "#dd4b39";
                            link.IconClass = "fab fa-google";
                            break;
                        case "Instagram":
                            link.IconColor = "#ac2bac";
                            link.IconClass = "fab fa-instagram";
                            break;
                        case "LinkedIn":
                            link.IconColor = "#0082ca";
                            link.IconClass = "fab fa-linkedin-in";
                            break;
                        case "GitHub":
                            link.IconColor = "#333333";
                            link.IconClass = "fab fa-github";
                            break;
                        case "YouTube":
                            link.IconColor = "#FF0000";
                            link.IconClass = "fab fa-youtube";
                            break;
                        default:
                            link.IconColor = "#333333";
                            break;
                    }

                    _context.Update(link);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SocialMediaLinkExists(link.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(link);
        }











        // صفحة تأكيد حذف رابط سوشيال ميديا
        public async Task<IActionResult> DeleteSocial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var socialLink = await _context.SocialMediaLinks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (socialLink == null)
            {
                return NotFound();
            }

            return View(socialLink);
        }

        // تنفيذ عملية الحذف
        [HttpPost, ActionName("DeleteSocial")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSocialConfirmed(int id)
        {
            var socialLink = await _context.SocialMediaLinks.FindAsync(id);
            if (socialLink != null)
            {
                _context.SocialMediaLinks.Remove(socialLink);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // طريقة مساعدة للتحقق من وجود الرابط
        private bool SocialMediaLinkExists(int id)
        {
            return _context.SocialMediaLinks.Any(e => e.Id == id);
        }

        // تنشيط/تعطيل رابط بدون حذفه
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var socialLink = await _context.SocialMediaLinks.FindAsync(id);
            if (socialLink == null)
            {
                return NotFound();
            }

            socialLink.IsActive = !socialLink.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}