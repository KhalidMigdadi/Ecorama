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
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        public ActionResult Details(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }




        public IActionResult ViewAllUsers()
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            var users = _context.Users
                .Include(u => u.Residences)
                .Include(u => u.Educations)
                .Include(u => u.Languages)
                .ToList();

            return View(users);
        }





        [HttpPost]
        public IActionResult ToggleActivation(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

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

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var allWorkShops = _context.Workshops.ToList();

            var latestWorkshops = allWorkShops
                .Where(w => w.Date != null)
                .OrderByDescending(w => w.Date);


            return View(latestWorkshops);
        }




        public IActionResult AddNewWorkshop()
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewWorkshop(WorkshopViewModel model)
        {


            if (ModelState.IsValid)
            {
                string imagePath = null;

                if (model.ImageFile != null)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/workshops");
                    Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);
                    }

                    imagePath = "/images/workshops/" + fileName;
                }

                // تحويل البيانات من ViewModel إلى Workshop entity
                var workshop = new Workshop
                {
                    Title = model.Title,
                    Description = model.Description,
                    Date = DateOnly.FromDateTime(model.Date),
                    WebSiteUrl = model.WebSiteUrl,
                    IsActive = model.IsActive,
                    Duration = model.Duration,
                    SeatsAvailable = model.SeatsAvailable,
                    ImageUrl = imagePath
                };

                _context.Workshops.Add(workshop);
                _context.SaveChanges();

                return RedirectToAction("seeAllWorkShop");
            }

            return View(model);
        }









        [HttpPost]
        public IActionResult DeleteWorkshop(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

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
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var currentWorkshop = _context.Workshops.Find(id);
            return View(currentWorkshop);
        }




        [HttpPost]
        public IActionResult EditWorkShop(Workshop workshop)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (!ModelState.IsValid)
                return View("EditWorkShop");

            _context.Workshops.Update(workshop);
            _context.SaveChanges();

            return RedirectToAction("seeAllWorkShop");
        }






        // partnet  Page




        public IActionResult ShowAllPartner()
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            var allPartners = _context.Partners.ToList();

            if (allPartners.Count == 0)
            {
                TempData["MSG_1"] = "Null";
                return View();
            }

            TempData["MSG_1"] = "NotNull";




            return View(allPartners);
        }





        public IActionResult AddNewPartner()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPartner_1(string name, string websiteUrl, IFormFile imageFile)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrEmpty(name) || imageFile == null || imageFile.Length == 0)
            {
                TempData["MSG"] = "الرجاء تعبئة جميع الحقول المطلوبة.";
                return View(nameof(AddNewPartner));
            }


            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["MSG"] = "صيغة الصورة غير مدعومة.";
                return View(nameof(AddNewPartner));
            }


            if (imageFile.Length > 2 * 1024 * 1024)
            {
                TempData["MSG"] = "حجم الصورة يجب أن لا يتجاوز 2MB.";
                return View(nameof(AddNewPartner));
            }


            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/partners");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }


            var imageUrl = "/uploads/partners/" + fileName;

            var newPartner = new Partner
            {
                Name = name,
                WebsiteUrl = websiteUrl,
                ImageUrl = imageUrl
            };

            _context.Partners.Add(newPartner);
            await _context.SaveChangesAsync();

            TempData["MSG"] = "تمت إضافة الشريك بنجاح.";
            return View(nameof(AddNewPartner));
        }













        public IActionResult UpdatePartner(int Id)
        {

            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            var Partner = _context.Partners.FirstOrDefault(p => p.Id == Id);

            if (Partner == null)
            {
                TempData["MSG"] = "الشريك غير موجود.";
                return RedirectToAction("ShowAllPartner");
            }

            return View(Partner);


        }


        [HttpPost]
        public IActionResult UpdatePartner_1(Partner Part, IFormFile imageFile)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrEmpty(Part.Name) || string.IsNullOrEmpty(Part.WebsiteUrl))
            {
                TempData["MSG_2"] = "الرجاء تعبئة جميع الحقول المطلوبة.";
                return View("UpdatePartner", Part);
            }

            var existingPartner = _context.Partners.FirstOrDefault(p => p.Id == Part.Id);

            if (existingPartner == null)
            {
                TempData["MSG_2"] = "الشريك غير موجود.";
                return RedirectToAction("ShowAllPartner");
            }

            existingPartner.Name = Part.Name;
            existingPartner.WebsiteUrl = Part.WebsiteUrl;

            // حفظ الصورة
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    TempData["MSG_2"] = "الرجاء اختيار صورة بصيغة JPG أو PNG فقط.";
                    return View("UpdatePartner", existingPartner);
                }

                if (imageFile.Length > 2 * 1024 * 1024)
                {
                    TempData["MSG_2"] = "حجم الصورة يجب ألا يتجاوز 2 ميجابايت.";
                    return View("UpdatePartner", existingPartner);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/partners");

                // إنشاء المجلد إذا مش موجود
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fullPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                existingPartner.ImageUrl = $"/images/partners/{fileName}";
            }

            _context.Partners.Update(existingPartner);
            _context.SaveChanges();

            TempData["MSG_2"] = "تم تحديث الشريك بنجاح.";
            return RedirectToAction("UpdatePartner", new { id = Part.Id });
        }





        [HttpPost]
        public IActionResult DeletePartner(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }



            var partner = _context.Partners.FirstOrDefault(p => p.Id == id);

            if (partner == null)
            {
                TempData["MSG_3"] = "الشريك غير موجود.";
                return RedirectToAction("ShowAllPartner");
            }

            if (!string.IsNullOrEmpty(partner.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", partner.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Partners.Remove(partner);
            _context.SaveChanges();

            TempData["MSG_3"] = "تم حذف الشريك بنجاح.";
            return RedirectToAction("ShowAllPartner");
        }


        public IActionResult ContactMassages()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            return View(_context.ContactUs.ToList());

        }




        // ================= News ==================

        public IActionResult News()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var news = _context.News.ToList();
            return View(news);
        }

        public IActionResult DetalisNews(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (id == null) return NotFound();

            var news = _context.News.FirstOrDefault(N => N.Id == id);

            if (news == null) return NotFound();

            return View(news);

        }


        public IActionResult CreateNews()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateNews(News news, IFormFile ImageFile)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (ImageFile != null)
            {
                string NewsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/news");
                string fileName = Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(NewsFolder, fileName);

                if (!Directory.Exists(NewsFolder))
                    Directory.CreateDirectory(NewsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                news.ImageUrl = fileName;
            }

            news.CreatedAt = DateTime.Now;
            news.IsActive = true;



            _context.News.Add(news);
            _context.SaveChanges();
            return RedirectToAction("News");
        }


        public IActionResult EditNews(int? id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (id == null) return NotFound();
            var news = _context.News.Find(id);
            if (news == null) return NotFound();

            ViewBag.newsTitle = news.Title;
            ViewBag.newsContent = news.Content;
            ViewBag.newsImageUrl = news.ImageUrl;
            ViewBag.newsIsActive = news.IsActive;

            return View(news);

        }

        [HttpPost]
        public IActionResult EditNews(News news, IFormFile ImageFile)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var n = _context.News.Find(news.Id);

            if (ImageFile != null)
            {
                string NewsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/news");
                string fileName = Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(NewsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                n.ImageUrl = fileName;
            }

            n.Title = news.Title;
            n.Content = news.Content;
            n.IsActive = news.IsActive;

            _context.News.Update(n);
            _context.SaveChanges();
            return RedirectToAction("News");

        }

        [HttpPost]
        public IActionResult DeleteNews(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var news = _context.News.Find(id);
            _context.News.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("News");
        }



    }




}


