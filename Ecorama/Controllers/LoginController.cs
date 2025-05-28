using System.Net.Mail;
using Ecorama.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;


namespace Ecorama.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDbContext _context;
        private readonly string _adminAuthCode = "Eco2025Admin"; // رمز التفويض للمشرفين - يفضل نقله إلى ملف التكوين

        public LoginController(MyDbContext context)
        {
            _context = context;
        }

        #region User Registration

        public async Task<IActionResult> Register()
        {
            // تحميل البيانات الأساسية للنموذج
            await LoadFormData();
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // التحقق من صحة حقل CustomLanguageName فقط إذا كان نوع اللغة "أخرى"
            if (model.Languages != null)
            {
                foreach (var lang in model.Languages.ToList())
                {
                    if (lang.LanguageName == "أخرى" && string.IsNullOrEmpty(lang.CustomLanguageName))
                    {
                        ModelState.AddModelError("Languages", "الرجاء إدخال اسم اللغة عند اختيار 'أخرى'");
                    }
                    // إذا لم تكن اللغة "أخرى"، فلا داعي للتحقق من CustomLanguageName
                    else if (lang.LanguageName != "أخرى")
                    {
                        // إزالة أي خطأ تحقق متعلق بـ CustomLanguageName إذا لم تكن اللغة "أخرى"
                        var key = $"Languages[{model.Languages.IndexOf(lang)}].CustomLanguageName";
                        if (ModelState.ContainsKey(key))
                        {
                            ModelState.Remove(key);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // التحقق من عدم وجود مستخدم بنفس الرقم الوطني
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == model.NationalId);
                if (existingUser != null)
                {
                    ModelState.AddModelError("NationalId", "هذا الرقم الوطني مسجل مسبقاً");
                    await LoadFormData();
                    return View(model);
                }

                // التحقق من عدم وجود مستخدم بنفس البريد الإلكتروني
                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل مسبقاً");
                    await LoadFormData();
                    return View(model);
                }

                var passwordHasher = new PasswordHasher<User>();


                // إنشاء كائن المستخدم الجديد
                var user = new User
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Birthdate = DateOnly.FromDateTime(model.Birthdate),
                    NationalId = model.NationalId,
                    Email = model.Email,
                    PasswordHash = model.Password,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    Role = "User",
                    IsActive = true
                };

                user.PasswordHash = passwordHasher.HashPassword(user, model.Password);


                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // إضافة معلومات السكن مع القرية يدوياً
                var residence = new Residence
                {
                    UserId = user.Id,
                    Governorate = model.Governorate,
                    District = model.District,
                    Village = model.Village,
                    IsCustomVillage = true  // تعيين القيمة إلى true لأن المستخدم أدخل القرية يدوياً
                };

                _context.Residences.Add(residence);

                // إضافة المعلومات التعليمية
                var education = new Education
                {
                    UserId = user.Id,
                    EducationLevel = model.EducationLevel,
                    ProgramType = model.ProgramType
                };

                _context.Educations.Add(education);

                // إضافة اللغات
                if (model.Languages != null && model.Languages.Count > 0)
                {
                    foreach (var lang in model.Languages)
                    {
                        // التعامل مع اللغات بشكل صحيح
                        if (lang.LanguageName == "أخرى" && !string.IsNullOrEmpty(lang.CustomLanguageName))
                        {
                            var language = new Language
                            {
                                UserId = user.Id,
                                LanguageName = lang.LanguageName,
                                CustomLanguageName = lang.CustomLanguageName,
                                ProficiencyLevel = lang.ProficiencyLevel
                            };
                            _context.Languages.Add(language);
                        }
                        else if (lang.LanguageName != "أخرى")
                        {
                            var language = new Language
                            {
                                UserId = user.Id,
                                LanguageName = lang.LanguageName,
                                CustomLanguageName = null,  // تعيين قيمة فارغة إذا لم تكن اللغة "أخرى"
                                ProficiencyLevel = lang.ProficiencyLevel
                            };
                            _context.Languages.Add(language);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح.";


                return RedirectToAction(nameof(Login));
            }

            // إذا كان هناك أخطاء في النموذج، يتم إعادة تحميل البيانات وعرض النموذج مرة أخرى
            await LoadFormData();
            return View(model);
        }

        // طريقة مساعدة لتحميل البيانات الأساسية للنموذج
        private async Task LoadFormData()
        {
            // تحميل الألوية التابعة لمحافظة إربد فقط
            var irbidGovernorate = await _context.Governorates
                .Include(g => g.Districts)
                .FirstOrDefaultAsync(g => g.Name == "إربد");

            if (irbidGovernorate != null)
            {
                var irbidDistricts = irbidGovernorate.Districts
                    .OrderBy(d => d.Name)
                    .ToList();

                ViewBag.IrbidDistricts = new SelectList(irbidDistricts, "Name", "Name");
            }
            else
            {
                // في حالة عدم وجود محافظة إربد في قاعدة البيانات
                ViewBag.IrbidDistricts = new SelectList(new List<string>());
            }

            // تحميل مستويات التعليم
            var educationLevels = new List<string>
            {
                "ثانوية عامة",
                "دبلوم",
                "بكالوريوس",
                "ماجستير",
                "دكتوراه"
            };
            ViewBag.EducationLevels = new SelectList(educationLevels);

            // تحميل أنواع البرامج
            var programTypes = new List<string>
            {
                "توجيه",
                "تدريب"
            };
            ViewBag.ProgramTypes = new SelectList(programTypes);

            // تحميل قائمة اللغات
            var languages = new List<string>
            {
                "العربية",
                "الإنجليزية",
                "الفرنسية",
                "الألمانية",
                "الإسبانية",
                "أخرى"
            };
            ViewBag.Languages = new SelectList(languages);

            // تحميل مستويات إتقان اللغة
            var proficiencyLevels = new List<string>
            {
                "اللغة الأم",
                "طلاقة",
                "متقدم",
                "متوسط",
                "مبتدئ"
            };
            ViewBag.ProficiencyLevels = new SelectList(proficiencyLevels);
        }

        #endregion

        #region Admin Registration

        //public IActionResult RegisterAdmin()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterAdmin(AdminRegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // التحقق من صحة رمز التفويض
        //        if (model.AuthorizationCode != _adminAuthCode)
        //        {
        //            ModelState.AddModelError("AuthorizationCode", "رمز التفويض غير صحيح");
        //            return View(model);
        //        }

        //        // التحقق من عدم وجود مستخدم بنفس البريد الإلكتروني
        //        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        //        if (existingUser != null)
        //        {
        //            ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل مسبقاً");
        //            return View(model);
        //        }

        //        // إنشاء كائن المستخدم المشرف الجديد
        //        var admin = new User
        //        {
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            Email = model.Email,
        //            PasswordHash = model.Password, // في الإنتاج يجب استخدام تشفير لكلمة المرور
        //            CreatedAt = DateTime.Now,
        //            Role = "Admin", // تعيين دور المستخدم كمشرف
        //            IsActive = true,
        //            // قيم افتراضية للحقول الإلزامية في جدول Users
        //            Gender = "غير محدد",
        //            Birthdate = DateOnly.FromDateTime(DateTime.Now),
        //            NationalId = $"ADMIN-{Guid.NewGuid().ToString().Substring(0, 8)}",
        //            PhoneNumber = "000000000"
        //        };

        //        _context.Users.Add(admin);
        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = "تم إنشاء حساب المشرف بنجاح.";
        //        return RedirectToAction(nameof(Login));
        //    }

        //    return View(model);
        //}

        #endregion

        #region Login

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // البحث عن المستخدم بالبريد الإلكتروني
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null) 
            {
                ModelState.AddModelError("", "البريد أو كلمة المرور غير صحيحة");
                return View(model);
            }

            // إنشاء PasswordHasher
            var passwordHasher = new PasswordHasher<User>();

            // تحقق من كلمة المرور
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "البريد أو كلمة المرور غير صحيحة");
                return View(model);
            }




            if (user.Email == "Ecorama@gmail.com" && user.PasswordHash == "Ecorama123")
            {
                HttpContext.Session.SetInt32("AdminId", user.Id);

            }
            else
            {
                // تخزين البيانات في الجلسة
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("MiddleName", user.MiddleName);
                HttpContext.Session.SetString("LastName", user.LastName);
                HttpContext.Session.SetString("NationalityId", user.NationalId);
                HttpContext.Session.SetString("Birthday", user.Birthdate.ToString());
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("Gender", user.Gender);
                HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber);
            }



            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath);
            }

            // توجيه المستخدم بناءً على دوره
            if (user.Email == "Ecorama@gmail.com" && user.PasswordHash == "Ecorama123")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // حذف كل بيانات الجلسة
            return RedirectToAction("Index","Home");
        }

        #endregion

        public IActionResult RegistrationSuccess()
        {
            return View();
        }


























        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HandleForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ResetError"] = "البريد الإلكتروني غير موجود!";
                return RedirectToAction("ForgotPassword");
            }

            // إنشاء رمز تحقق عشوائي
            Random rand = new Random();
            int verificationCode = rand.Next(100000, 999999);

            // تخزين الرمز والبريد الإلكتروني في الجلسة
            HttpContext.Session.SetString("ResetCode", verificationCode.ToString());
            HttpContext.Session.SetString("ResetEmail", email);

            // إرسال رمز التحقق عبر البريد الإلكتروني
            await SendEmailAsync(email, "رمز إعادة تعيين كلمة المرور", $"رمز إعادة التعيين الخاص بك هو: {verificationCode}");

            TempData["VerificationMessage"] = "A verification code has been sent to your email!";

            return RedirectToAction("VerifyResetCode");
        }

        public IActionResult VerifyResetCode()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HandleVerifyCode(string code)
        {
            var storedCode = HttpContext.Session.GetString("ResetCode");
            var email = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            if (storedCode != code)
            {
                TempData["CodeError"] = "رمز التحقق غير صحيح!";
                return RedirectToAction("VerifyResetCode");
            }

            return RedirectToAction("NewPassword");
        }

        public IActionResult NewPassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["PasswordError"] = "انتهت الجلسة. يرجى المحاولة مرة أخرى.";
                return RedirectToAction("ForgotPassword");
            }

            if (newPassword != confirmPassword)
            {
                TempData["PasswordError"] = "كلمات المرور غير متطابقة!";
                return RedirectToAction("NewPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["PasswordError"] = "المستخدم غير موجود!";
                return RedirectToAction("ForgotPassword");
            }

            // تشفير وتخزين كلمة المرور الجديدة باستخدام PasswordHasher
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

            await _context.SaveChangesAsync();

            TempData["PasswordSuccess"] = "تم تحديث كلمة المرور بنجاح!";
            return RedirectToAction("Login");
        }

        // استبدال دالة SendEmail بهذه الدالة المُحدّثة

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // إنشاء رسالة البريد الإلكتروني
                var message = new MimeMessage();

                // تعيين المرسل
                message.From.Add(new MailboxAddress("إعادة تعيين كلمة المرور", "cafeuse18@gmail.com"));

                // تعيين المستلم
                message.To.Add(new MailboxAddress("المستلم", toEmail)); // يمكنك إضافة اسم المستلم هنا

                // تعيين الموضوع والنص
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                // إرسال البريد الإلكتروني
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // الاتصال بالخادم باستخدام TLS عبر المنفذ 587
                    await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);

                    // مصادقة البريد الإلكتروني باستخدام كلمة مرور التطبيق
                    await client.AuthenticateAsync("cafeuse18@gmail.com", "mecd idlj jnxt vrrw");

                    // إرسال البريد الإلكتروني
                    await client.SendAsync(message);

                    // قطع الاتصال
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("تم إرسال البريد الإلكتروني بنجاح!");
            }
            catch (Exception ex)
            {
                // طباعة رسالة الخطأ
                Console.WriteLine($"خطأ في إرسال البريد الإلكتروني: {ex.Message}");
                Console.WriteLine($"تفاصيل الاستثناء: {ex}");
            }
        }










    }
}