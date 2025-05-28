using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecorama.Models;


namespace Ecorama.Controllers
{
    public class SliderController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/SliderItems
        public async Task<IActionResult> Index()
        {
            return View(await _context.SliderItems.OrderBy(s => s.Order).ToListAsync());
        }








        // GET: Admin/SliderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sliderItem = await _context.SliderItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sliderItem == null)
            {
                return NotFound();
            }

            return View(sliderItem);
        }











        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile imageFile, SliderItem sliderItem)
        {
            // معالجة الصورة المرفوعة أولاً
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "slider");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                sliderItem.ImageUrl = "/images/slider/" + uniqueFileName; // بدون ~
                sliderItem.ImageFilePath = filePath;
            }
            else
            {
                // إذا ما تم رفع صورة، ضيف خطأ مخصص
                ModelState.AddModelError("ImageUrl", "يرجى رفع صورة للعنصر.");
            }


            _context.Add(sliderItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }























        // GET: Admin/SliderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sliderItem = await _context.SliderItems.FindAsync(id);
            if (sliderItem == null)
            {
                return NotFound();
            }
            return View(sliderItem);
        }

        // POST: Admin/SliderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageUrl,Title,Description,IsActive,Order,ImageFilePath")] SliderItem sliderItem, IFormFile imageFile)
        {
            if (id != sliderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // معالجة الصورة الجديدة إذا تم تحميلها
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // حذف الصورة القديمة إذا وجدت
                        if (!string.IsNullOrEmpty(sliderItem.ImageFilePath) && System.IO.File.Exists(sliderItem.ImageFilePath))
                        {
                            System.IO.File.Delete(sliderItem.ImageFilePath);
                        }

                        // إنشاء مسار فريد للصورة الجديدة
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "slider");

                        // التأكد من وجود المجلد
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // إنشاء اسم فريد للملف
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // حفظ الملف
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // تحديث مسار الصورة في الكائن
                        sliderItem.ImageUrl = "~/images/slider/" + uniqueFileName;
                        sliderItem.ImageFilePath = filePath;
                    }

                    _context.Update(sliderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderItemExists(sliderItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sliderItem);
        }

        // GET: Admin/SliderItems/Delete/5


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sliderItem = await _context.SliderItems.FindAsync(id);

            if (sliderItem != null)
            {
                if (!string.IsNullOrEmpty(sliderItem.ImageFilePath) && System.IO.File.Exists(sliderItem.ImageFilePath))
                {
                    System.IO.File.Delete(sliderItem.ImageFilePath);
                }

                _context.SliderItems.Remove(sliderItem);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }









        // تغيير ترتيب العناصر
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int id, int newOrder)
        {
            var sliderItem = await _context.SliderItems.FindAsync(id);
            if (sliderItem == null)
            {
                return NotFound();
            }

            sliderItem.Order = newOrder;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // تغيير حالة التنشيط
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var sliderItem = await _context.SliderItems.FindAsync(id);
            if (sliderItem == null)
            {
                return NotFound();
            }

            sliderItem.IsActive = !sliderItem.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { isActive = sliderItem.IsActive });
        }

        private bool SliderItemExists(int id)
        {
            return _context.SliderItems.Any(e => e.Id == id);
        }
    }
}

