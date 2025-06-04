
using Ecorama.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;



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
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            var allUsers = _context.Users.Where(u => u.Role == "User").Count();
            ViewBag.AllUsers = allUsers;

            var allWorkshops = _context.Workshops.Count();
            ViewBag.AllWorkshops = allWorkshops;

            var allPartners = _context.Partners.Count();
            ViewBag.AllPartners = allPartners;

            var allCourses = _context.Courses.Count();
            ViewBag.AllCourses = allCourses;

            var allBookings = _context.RoomBookings.Count();
            ViewBag.allBookings = allBookings;



            var usersTable = _context.Users.Select(u => new Ecorama.Models.User
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).Take(8).ToList();


            var allWorkShops = _context.Workshops.Select(w => new Ecorama.Models.Workshop
            {
                Title = w.Title,
                StartDate = w.StartDate,
                SeatsAvailable = w.SeatsAvailable

            }).Take(4).ToList();


            var dashVM = new AdminDashViewModel
            {
                users = usersTable,
                workshops = allWorkShops
            };


            return View(dashVM);
        }

        public ActionResult Details(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }




        public IActionResult ViewAllUsers()
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
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


        public async Task<IActionResult> ExportUsersToExcel()
        {
            var users = await (from u in _context.Users.Where(u => u.Role == "User")
                               join r in _context.Residences on u.Id equals r.UserId into res
                               from r in res.DefaultIfEmpty()
                               join e in _context.Educations on u.Id equals e.UserId into edu
                               from e in edu.DefaultIfEmpty()
                               join l in _context.Languages on u.Id equals l.UserId into lang
                               from l in lang.DefaultIfEmpty()
                               select new UserExportViewModel
                               {
                                   Id = u.Id,
                                   FirstName = u.FirstName,
                                   MiddleName = u.MiddleName,
                                   LastName = u.LastName,
                                   Gender = u.Gender,
                                   Birthdate = u.Birthdate.ToDateTime(TimeOnly.MinValue),
                                   NationalId = u.NationalId,
                                   Email = u.Email,
                                   PhoneNumber = u.PhoneNumber,
                                   Governorate = r != null ? r.Governorate : null,
                                   District = r != null ? r.District : null,
                                   Village = r != null ? r.Village : null,
                                   EducationLevel = e != null ? e.EducationLevel : null,
                                   ProgramType = e != null ? e.ProgramType : null,
                                   LanguageName = l != null ? l.LanguageName : null,
                                   CustomLanguageName = l != null ? l.CustomLanguageName : null,
                                   ProficiencyLevel = l != null ? l.ProficiencyLevel : null
                               }).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");

                // العناوين
                var headers = new[]
                {
            "Id", "First Name", "Middle Name", "Last Name", "Gender", "Birthdate", "National ID",
            "Email", "Phone", "Governorate", "District", "Village", "Education Level", "Program Type",
            "Language Name", "Custom Language", "Proficiency"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Column(i + 1).AdjustToContents();
                }

                // البيانات

                for (int i = 0; i < users.Count; i++)
                {
                    var user = users[i];
                    worksheet.Cell(i + 2, 1).Value = user.Id;
                    worksheet.Cell(i + 2, 2).Value = user.FirstName;
                    worksheet.Cell(i + 2, 3).Value = user.MiddleName;
                    worksheet.Cell(i + 2, 4).Value = user.LastName;
                    worksheet.Cell(i + 2, 5).Value = user.Gender;
                    worksheet.Cell(i + 2, 6).Value = user.Birthdate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 7).Value = user.NationalId;
                    worksheet.Cell(i + 2, 8).Value = user.Email;
                    worksheet.Cell(i + 2, 9).Value = user.PhoneNumber;
                    worksheet.Cell(i + 2, 10).Value = user.Governorate;
                    worksheet.Cell(i + 2, 11).Value = user.District;
                    worksheet.Cell(i + 2, 12).Value = user.Village;
                    worksheet.Cell(i + 2, 13).Value = user.EducationLevel;
                    worksheet.Cell(i + 2, 14).Value = user.ProgramType;
                    worksheet.Cell(i + 2, 15).Value = user.LanguageName;
                    worksheet.Cell(i + 2, 16).Value = user.CustomLanguageName;
                    worksheet.Cell(i + 2, 17).Value = user.ProficiencyLevel;
                }

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Column(i + 1).AdjustToContents();
                }


                // تصدير الملف
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "UsersExport.xlsx");
                }
            }
        }



        [HttpPost]
        public IActionResult ToggleActivation(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");

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

            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var allWorkShops = _context.Workshops.ToList();

            var latestWorkshops = allWorkShops
                .Where(w => w.StartDate != null)
                .OrderByDescending(w => w.StartDate);


            return View(latestWorkshops);
        }





        public IActionResult ExportWorkshopsToExcel()
        {
            var workshops = _context.Workshops.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Workshops");

                // رؤوس الأعمدة بعد التعديل
                worksheet.Cell(1, 1).Value = "العنوان | Title";
                worksheet.Cell(1, 2).Value = "الوصف | Description";
                worksheet.Cell(1, 3).Value = "تاريخ البدء | Start Date";
                worksheet.Cell(1, 4).Value = "تاريخ الانتهاء | End Date";
                worksheet.Cell(1, 5).Value = "المنظمة | Organization";
                worksheet.Cell(1, 6).Value = "الرابط | Website URL";
                worksheet.Cell(1, 7).Value = "الحالة | IsActive";

                // تنسيق رؤوس الأعمدة
                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // تعبئة البيانات
                for (int i = 0; i < workshops.Count; i++)
                {
                    var row = i + 2;
                    var ws = workshops[i];

                    worksheet.Cell(row, 1).Value = ws.Title ?? "";
                    worksheet.Cell(row, 2).Value = ws.Description ?? "";
                    worksheet.Cell(row, 3).Value = ws.StartDate?.ToString("yyyy-MM-dd") ?? "";
                    worksheet.Cell(row, 4).Value = ws.EndDate?.ToString("yyyy-MM-dd") ?? "";
                    worksheet.Cell(row, 5).Value = ws.Organization ?? "";
                    worksheet.Cell(row, 6).Value = ws.WebSiteUrl ?? "";
                    worksheet.Cell(row, 7).Value = ws.IsActive ? "فعّالة | Active" : "غير فعّالة | Inactive";

                    // تنسيق لكل صف
                    for (int col = 1; col <= 7; col++)
                    {
                        worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                }

                // Auto fit لكل الأعمدة
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Workshops_Styled.xlsx");
                }
            }
        }















        [HttpPost]
        public async Task<IActionResult> ToggleWorkshopStatus(int id, bool isActive)
        {
            var workshop = await _context.Workshops.FindAsync(id);
            if (workshop == null)
            {
                return NotFound();
            }

            workshop.IsActive = isActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(seeAllWorkShop)); // عدّل حسب اسم الأكشن الأساسي
        }


        public IActionResult AddNewWorkshop()
        {

            int? adminId = HttpContext.Session.GetInt32("UserId");

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
                    Organization = model.Organsization,
                    StartDate = DateOnly.FromDateTime(model.StartDate),
                    EndDate = DateOnly.FromDateTime(model.EndDate),
                    WebSiteUrl = model.WebSiteUrl,
                    IsActive = model.IsActive,
                    Duration = model.Duration,
                    SeatsAvailable = model.SeatsAvailable,
                    ImageUrl = imagePath
                };

                workshop.IsActive = true;

                _context.Workshops.Add(workshop);
                _context.SaveChanges();

                return RedirectToAction("seeAllWorkShop");
            }

            return View(model);
        }









        [HttpPost]
        public IActionResult DeleteWorkshop(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");

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
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var currentWorkshop = _context.Workshops.Find(id);
            if (currentWorkshop == null)
            {
                return NotFound();
            }

            return View(currentWorkshop);
        }


        [HttpPost]
        public async Task<IActionResult> EditWorkShop(Workshop workshop, IFormFile? ImageFile, string OldImageUrl)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (!ModelState.IsValid)
                return View("EditWorkShop", workshop);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "workshops");
                Directory.CreateDirectory(uploadsFolder); // تأكد من وجود المجلد

                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // تحديث الصورة الجديدة
                workshop.ImageUrl = "/img/workshops/" + fileName;
            }
            else
            {
                // الاحتفاظ بالصورة القديمة
                workshop.ImageUrl = OldImageUrl;
            }



            _context.Workshops.Update(workshop);
            await _context.SaveChangesAsync();

            return RedirectToAction("seeAllWorkShop");
        }









        // partnet  Page




        public IActionResult ShowAllPartner()
        {

            int? adminId = HttpContext.Session.GetInt32("UserId");

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
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPartner_1(string name, string websiteUrl, IFormFile imageFile)
        {

            int? adminId = HttpContext.Session.GetInt32("UserId");

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

            int? adminId = HttpContext.Session.GetInt32("UserId");

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
            int? adminId = HttpContext.Session.GetInt32("UserId");

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
            int? adminId = HttpContext.Session.GetInt32("UserId");

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
            int? adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            return View(_context.ContactUs.ToList());

        }

        public IActionResult ExportContactMessagesToExcel()
        {
            var messages = _context.ContactUs.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Contact Messages");

                // ===== عناوين الأعمدة =====
                var headers = new[] { "#", "Full Name", "Email", "Subject", "Message", "Received At" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontColor = XLColor.DarkBlue;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== البيانات =====
                for (int i = 0; i < messages.Count; i++)
                {
                    var msg = messages[i];

                    worksheet.Cell(i + 2, 1).Value = i + 1;
                    worksheet.Cell(i + 2, 2).Value = msg.FullName;
                    worksheet.Cell(i + 2, 3).Value = msg.Email;
                    worksheet.Cell(i + 2, 4).Value = string.IsNullOrEmpty(msg.Subject) ? "-" : msg.Subject;
                    worksheet.Cell(i + 2, 5).Value = msg.Message;
                    worksheet.Cell(i + 2, 6).Value = msg.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

                    // تنسيق الخلايا
                    for (int col = 1; col <= 6; col++)
                    {
                        var cell = worksheet.Cell(i + 2, col);
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        cell.Style.Alignment.WrapText = true; // لف النص داخل الخلية
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                // ===== جعل الأعمدة تلائم المحتوى =====
                worksheet.Columns().AdjustToContents();

                // تصدير الملف
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ContactMessages.xlsx");
                }
            }
        }





        // ================= News ==================

        public IActionResult News()
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var news = _context.News.ToList();
            return View(news);
        }


        public IActionResult ExportNewsToExcel()
        {
            var newsList = _context.News.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("News");

                // ===== عناوين الأعمدة =====
                var headers = new[] { "#", "Title", "Content", "Created At", "Status" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Font.FontColor = XLColor.DarkBlue;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== البيانات =====
                for (int i = 0; i < newsList.Count; i++)
                {
                    var news = newsList[i];

                    worksheet.Cell(i + 2, 1).Value = i + 1;
                    worksheet.Cell(i + 2, 2).Value = news.Title;
                    worksheet.Cell(i + 2, 3).Value = news.Content;
                    worksheet.Cell(i + 2, 4).Value = news.CreatedAt?.ToString("yyyy/MM/dd") ?? "-";
                    worksheet.Cell(i + 2, 5).Value = news.IsActive ? "نشط" : "غير نشط";

                    // تنسيق الخلايا
                    for (int col = 1; col <= 5; col++)
                    {
                        var cell = worksheet.Cell(i + 2, col);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                // ===== جعل الأعمدة تلائم المحتوى =====
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "News.xlsx");
                }
            }
        }



        public IActionResult DetalisNews(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
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
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateNews(News news, IFormFile ImageFile)
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
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
            int? adminId = HttpContext.Session.GetInt32("UserId");
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
            int? adminId = HttpContext.Session.GetInt32("UserId");
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
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var news = _context.News.Find(id);
            _context.News.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("News");
        }



 



         public async Task<IActionResult> WorkshopRegistrationsUsers()
        {
            int? adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }


            try
            {
                var registrations = await _context.WorkshopRegistrations
                    .Include(w => w.Workshop)
                    .Include(w => w.User)
                    .OrderByDescending(w => w.RegisteredAt)
                    .ToListAsync();

                ViewBag.TotalCount = registrations.Count;
                ViewBag.TodayCount = registrations.Count(x => x.RegisteredAt?.Date == DateTime.Today);
                ViewBag.ThisWeekCount = registrations.Count(x => x.RegisteredAt >= DateTime.Today.AddDays(-7));
                ViewBag.ThisMonthCount = registrations.Count(x => x.RegisteredAt >= DateTime.Today.AddDays(-30));

                return View(registrations);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["Error"] = "حدث خطأ في تحميل البيانات";
                return View(new List<WorkshopRegistration>());
            }
        }




        // GET: يعرض كل المشرفين
        public IActionResult AllAdmins()
        {
            var admins = _context.Users.Where(u => u.Role == "Partner").ToList();
            return View(admins);
        }

        // POST: لتعطيل المشرف (تغيير IsActive إلى false)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAdmin(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != "Partner")
            {
                return NotFound();
            }

            admin.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AllAdmins));
        }

        // POST: لتفعيل المشرف (تغيير IsActive إلى true) — اختياري إذا تريد زر تفعيل أيضاً
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAdmin(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != "Partner")
            {
                return NotFound();
            }

            admin.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AllAdmins));
        }


    }

}


