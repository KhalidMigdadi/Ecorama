using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSocial(SocialMediaLink link)
        {
            if (ModelState.IsValid)
            {
                // تحديد اللون بناءً على اسم الشبكة الاجتماعية
                link.IconColor = link.Name switch
                {
                    "Facebook" => "#3b5998",
                    "Twitter" => "#55acee",
                    "Google" => "#dd4b39",
                    "Instagram" => "#ac2bac",
                    "LinkedIn" => "#0082ca",
                    "GitHub" => "#333333",
                    _ => "#333333"  // الافتراضي إذا لم يتطابق الاسم مع أي شبكة
                };

                link.CreatedAt = DateTime.Now;
                _context.Add(link);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
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
                    // تحديد اللون بناءً على اسم الشبكة الاجتماعية
                    link.IconColor = link.Name switch
                    {
                        "Facebook" => "#3b5998",
                        "Twitter" => "#55acee",
                        "Google" => "#dd4b39",
                        "Instagram" => "#ac2bac",
                        "LinkedIn" => "#0082ca",
                        "GitHub" => "#333333",
                        _ => "#333333"  // الافتراضي إذا لم يتطابق الاسم مع أي شبكة
                    };

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