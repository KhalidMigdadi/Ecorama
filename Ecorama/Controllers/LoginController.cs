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

                // البحث عن اللواء في قاعدة البيانات
                var district = await _context.Districts.FirstOrDefaultAsync(d => d.Name == model.District);

                // إضافة معلومات السكن - بدون إشارة إلى القرية
                var residence = new Residence
                {
                    UserId = user.Id,
                    Governorate = model.Governorate,
                    District = model.District,
                    IsCustomVillage = true  // دائمًا true لأن لا توجد قرية مرتبطة
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
                        var language = new Language
                        {
                            UserId = user.Id,
                            LanguageName = lang.LanguageName,
                            CustomLanguageName = lang.LanguageName == "other" ? lang.CustomLanguageName : null,
                            ProficiencyLevel = lang.ProficiencyLevel
                        };
                        _context.Languages.Add(language);
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


        // طريقة مساعدة لتحميل البيانات الأساسية للنموذج - نحذف تحميل القرى
        private async Task LoadFormData()
        {
            // تحميل المحافظات
            var governorates = await _context.Governorates.ToListAsync();
            ViewBag.Governorates = new SelectList(governorates, "Name", "Name");

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


        // GET: Api/GetDistricts
        [HttpGet("api/[controller]/GetDistricts")]
        public async Task<IActionResult> GetDistricts(string governorate)
        {
            if (string.IsNullOrEmpty(governorate))
            {
                return BadRequest("يجب تحديد المحافظة");
            }

            var governorateEntity = await _context.Governorates
                .Include(g => g.Districts)
                .FirstOrDefaultAsync(g => g.Name == governorate);

            if (governorateEntity == null)
            {
                return NotFound("المحافظة غير موجودة");
            }

            var districts = governorateEntity.Districts.OrderBy(d => d.Name)
                .Select(d => new { id = d.DistrictId, name = d.Name });

            return Ok(districts);
        }

        // GET: Api/GetVillages
        [HttpGet("api/[controller]/GetVillages")]
        public async Task<IActionResult> GetVillages(int districtId)
        {
            if (districtId <= 0)
            {
                return BadRequest("يجب تحديد اللواء");
            }

            var districtEntity = await _context.Districts
                .Include(d => d.Villages)
                .FirstOrDefaultAsync(d => d.DistrictId == districtId);

            if (districtEntity == null)
            {
                return NotFound("اللواء غير موجود");
            }

            var villages = districtEntity.Villages.OrderBy(v => v.Name)
                .Select(v => new { id = v.VillageId, name = v.Name });

            return Ok(villages);
        }
    }
}

