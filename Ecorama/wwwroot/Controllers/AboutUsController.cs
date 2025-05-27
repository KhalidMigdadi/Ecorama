using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class AboutUsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AboutUsController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: AboutUs/Manage
        public async Task<IActionResult> Manage()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            var aboutUsItems = await _context.AboutUs.OrderBy(x => x.CreatedAt).ToListAsync();
            return View(aboutUsItems);
        }

        // GET: AboutUs/Create
        public IActionResult CreateSlider()
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            return View();
        }

        // POST: AboutUs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSlider(AboutU aboutU, IFormFile? imageFile)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "about");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    aboutU.ImageUrl = "/images/about/" + uniqueFileName;
                }

                aboutU.CreatedAt = DateTime.Now;
                _context.AboutUs.Add(aboutU);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة العنصر بنجاح!";
                return RedirectToAction(nameof(Manage));
            }

            return View(aboutU);
        }






        // GET: AboutUs/Edit/5
        public async Task<IActionResult> EditSlider(int? id)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            if (id == null)
            {
                return NotFound();
            }

            var aboutU = await _context.AboutUs.FindAsync(id);
            if (aboutU == null)
            {
                return NotFound();
            }

            return View(aboutU);
        }

        // POST: AboutUs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSlider(int id, AboutU aboutU, IFormFile? imageFile)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            if (id != aboutU.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAboutU = await _context.AboutUs.FindAsync(id);
                    if (existingAboutU == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingAboutU.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingAboutU.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "about");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingAboutU.ImageUrl = "/images/about/" + uniqueFileName;
                    }

                    existingAboutU.Title = aboutU.Title;
                    existingAboutU.Description = aboutU.Description;


                    existingAboutU.UpdatedAt = DateTime.Now;



                    _context.Update(existingAboutU);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تم تحديث العنصر بنجاح!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutUExists(aboutU.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(aboutU);
        }

        // GET: AboutUs/Details/5
        public async Task<IActionResult> DetailsSlider(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aboutU = await _context.AboutUs.FirstOrDefaultAsync(m => m.Id == id);
            if (aboutU == null)
            {
                return NotFound();
            }

            return View(aboutU);
        }

        // GET: AboutUs/Delete/5
        public async Task<IActionResult> DeleteSlider(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aboutU = await _context.AboutUs.FirstOrDefaultAsync(m => m.Id == id);
            if (aboutU == null)
            {
                return NotFound();
            }

            return View(aboutU);
        }

        // POST: AboutUs/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aboutU = await _context.AboutUs.FindAsync(id);
            if (aboutU != null)
            {
                // Delete associated image
                if (!string.IsNullOrEmpty(aboutU.ImageUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, aboutU.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.AboutUs.Remove(aboutU);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف العنصر بنجاح!";
            }

            return RedirectToAction(nameof(Manage));
        }

        private bool AboutUExists(int id)
        {
            return _context.AboutUs.Any(e => e.Id == id);
        }






        // AJAX endpoint to reorder slides
        [HttpPost]
        public async Task<IActionResult> ReorderSlides([FromBody] List<int> slideIds)
        {
            try
            {
                var slides = await _context.AboutUs.Where(a => slideIds.Contains(a.Id)).ToListAsync();

                for (int i = 0; i < slideIds.Count; i++)
                {
                    var slide = slides.FirstOrDefault(s => s.Id == slideIds[i]);
                    if (slide != null)
                    {
                        // You can add an Order field to your model to maintain order
                        // slide.Order = i;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
