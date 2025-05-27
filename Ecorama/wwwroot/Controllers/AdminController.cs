
using Ecorama.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;



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


            var allUsers = _context.Users.Where(u => u.Role == "User").Count();
            ViewBag.AllUsers = allUsers;

            var allWorkshops = _context.Workshops.Count();
            ViewBag.AllWorkshops = allWorkshops;

            var allPartners = _context.Partners.Count();
            ViewBag.AllPartners = allPartners;

            var allCourses = _context.Courses.Count();
            ViewBag.AllCourses = allCourses;



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
                Date = w.Date,
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
        public IActionResult ExportAllUsersToPdf()
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

            return GenerateUsersPdf(users, "جميع_المستخدمين");
        }

        [HttpPost]
        public IActionResult ExportUserToPdf(int id)
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var user = _context.Users
                .Include(u => u.Residences)
                .Include(u => u.Educations)
                .Include(u => u.Languages)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return GenerateUsersPdf(new List<User> { user }, $"تفاصيل_المستخدم_{user.FirstName}_{user.LastName}");
        }

        private FileResult GenerateUsersPdf(List<User> users, string fileName)
        {
            using (var stream = new MemoryStream())
            {
                // إنشاء مستند PDF
                var document = new Document(PageSize.A4, 20, 20, 30, 30);
                var writer = PdfWriter.GetInstance(document, stream);

                // تعيين اتجاه الكتابة من اليمين لليسار
                writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                document.Open();

                // إعداد الخط العربي مع مسارات متعددة
                BaseFont arabicFont = null;
                string[] fontPaths = {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "NotoSansArabic-Regular.ttf"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "tahoma.ttf"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "calibri.ttf")
            };

                foreach (string fontPath in fontPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(fontPath))
                        {
                            arabicFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // في حالة عدم العثور على خط مناسب، استخدم خط افتراضي
                if (arabicFont == null)
                {
                    try
                    {
                        arabicFont = BaseFont.CreateFont(BaseFont.HELVETICA, "Cp1256", BaseFont.NOT_EMBEDDED);
                    }
                    catch
                    {
                        arabicFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    }
                }

                var titleFont = new iTextSharp.text.Font(arabicFont, 18, iTextSharp.text.Font.BOLD);
                var headerFont = new iTextSharp.text.Font(arabicFont, 12, iTextSharp.text.Font.BOLD);
                var normalFont = new iTextSharp.text.Font(arabicFont, 10, iTextSharp.text.Font.NORMAL);

                // عنوان المستند مع معالجة النص العربي
                var title = new Paragraph(ReverseArabicText("ريرقت تانايب نيمدختسملا"), titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                // معلومات التقرير
                var reportInfo = new Paragraph(ReverseArabicText($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm}"), normalFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 10
                };
                document.Add(reportInfo);

                var userCount = new Paragraph(ReverseArabicText($"عدد المستخدمين: {users.Count}"), normalFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 20
                };
                document.Add(userCount);

                // إضافة بيانات كل مستخدم
                foreach (var user in users)
                {
                    AddUserToPdf(document, user, headerFont, normalFont);
                    if (users.Count > 1 && user != users.Last())
                    {
                        document.NewPage();
                    }
                }

                document.Close();
                return File(stream.ToArray(), "application/pdf", $"{fileName}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        // دالة لمعالجة النصوص العربية
        private string ReverseArabicText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // إذا كان النص يحتوي على أرقام أو أحرف إنجليزية مع العربية
            var words = text.Split(' ');
            var reversedWords = new List<string>();

            foreach (var word in words)
            {
                if (ContainsArabic(word) && !ContainsEnglishOrNumbers(word))
                {
                    // عكس الكلمات العربية فقط
                    reversedWords.Insert(0, word);
                }
                else
                {
                    // الاحتفاظ بالأرقام والإنجليزية في مكانها
                    reversedWords.Add(word);
                }
            }

            return string.Join(" ", reversedWords);
        }

        private bool ContainsArabic(string text)
        {
            return text.Any(c => c >= 0x0600 && c <= 0x06FF);
        }

        private bool ContainsEnglishOrNumbers(string text)
        {
            return text.Any(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'));
        }

        private void AddUserToPdf(Document document, User user, iTextSharp.text.Font headerFont, iTextSharp.text.Font normalFont)
        {
            // معلومات المستخدم الأساسية
            var userHeader = new Paragraph(ReverseArabicText($"بيانات المستخدم: {user.FirstName} {user.LastName}"), headerFont)
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 10
            };
            document.Add(userHeader);

            // جدول المعلومات الأساسية
            var basicInfoTable = new PdfPTable(2) { WidthPercentage = 100 };
            basicInfoTable.SetWidths(new float[] { 1, 2 });
            basicInfoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

            AddTableRow(basicInfoTable, "الاسم الكامل", $"{user.FirstName} {user.MiddleName} {user.LastName}", headerFont, normalFont);
            AddTableRow(basicInfoTable, "الجنس", user.Gender ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "تاريخ الميلاد", user.Birthdate.ToString("yyyy-MM-dd") ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "الرقم الوطني", user.NationalId ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "البريد الإلكتروني", user.Email ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "رقم الهاتف", user.PhoneNumber ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "الدور", user.Role ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "تاريخ الإنشاء", user.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "", headerFont, normalFont);
            AddTableRow(basicInfoTable, "الحالة", user.IsActive ? "مفعّل" : "معطّل", headerFont, normalFont);

            document.Add(basicInfoTable);
            document.Add(new Paragraph(" ", normalFont) { SpacingAfter = 10 });

            // معلومات السكن
            if (user.Residences != null && user.Residences.Any())
            {
                var residenceHeader = new Paragraph(ReverseArabicText("معلومات السكن"), headerFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 5
                };
                document.Add(residenceHeader);

                var residenceTable = new PdfPTable(4) { WidthPercentage = 100 };
                residenceTable.SetWidths(new float[] { 1, 1, 1, 1 });
                residenceTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                // عناوين الأعمدة
                residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("المحافظة"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("اللواء"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("القرية/البلدة"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("نوع البلدة"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var residence in user.Residences)
                {
                    residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(residence.Governorate ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(residence.District ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(residence.Village ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    residenceTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(residence.IsCustomVillage == true ? "مدخلة يدوياً" : "قائمة النظام"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(residenceTable);
                document.Add(new Paragraph(" ", normalFont) { SpacingAfter = 10 });
            }

            // المعلومات التعليمية
            if (user.Educations != null && user.Educations.Any())
            {
                var educationHeader = new Paragraph(ReverseArabicText("المعلومات التعليمية"), headerFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 5
                };
                document.Add(educationHeader);

                var educationTable = new PdfPTable(2) { WidthPercentage = 100 };
                educationTable.SetWidths(new float[] { 1, 1 });
                educationTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                // عناوين الأعمدة
                educationTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("المستوى التعليمي"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                educationTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("نوع البرنامج"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var education in user.Educations)
                {
                    educationTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(education.EducationLevel ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    educationTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(education.ProgramType ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(educationTable);
                document.Add(new Paragraph(" ", normalFont) { SpacingAfter = 10 });
            }

            // معلومات اللغات
            if (user.Languages != null && user.Languages.Any())
            {
                var languageHeader = new Paragraph(ReverseArabicText("اللغات"), headerFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 5
                };
                document.Add(languageHeader);

                var languageTable = new PdfPTable(2) { WidthPercentage = 100 };
                languageTable.SetWidths(new float[] { 1, 1 });
                languageTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                // عناوين الأعمدة
                languageTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("اللغة"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                languageTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText("مستوى الإتقان"), headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var language in user.Languages)
                {
                    string langName = string.IsNullOrEmpty(language.CustomLanguageName)
                        ? language.LanguageName ?? ""
                        : $"{language.LanguageName} - {language.CustomLanguageName}";

                    languageTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(langName), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    languageTable.AddCell(new PdfPCell(new Phrase(ReverseArabicText(language.ProficiencyLevel ?? ""), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(languageTable);
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, iTextSharp.text.Font headerFont, iTextSharp.text.Font normalFont)
        {
            table.AddCell(new PdfPCell(new Phrase(ReverseArabicText(label), headerFont))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = BaseColor.LIGHT_GRAY
            });
            table.AddCell(new PdfPCell(new Phrase(ReverseArabicText(value), normalFont))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
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


