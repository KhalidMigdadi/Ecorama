using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDbContext _context;

        public LoginController(MyDbContext context)
        {
            _context = context;
        }

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

                // إعادة التوجيه إلى صفحة تأكيد التسجيل
                return RedirectToAction(nameof(RegistrationSuccess));
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

        public IActionResult RegistrationSuccess()
        {
            return View();
        }
    }
}