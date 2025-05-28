using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Ecorama.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MyDbContext _context;

        public ProfileController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Get the user with all related data
            var user = _context.Users
                .Include(u => u.Residences)
                .Include(u => u.Educations)
                .Include(u => u.Languages)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "Login");

            return View(user);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Get the user with all related data
            var user = _context.Users
                .Include(u => u.Residences)
                .Include(u => u.Educations)
                .Include(u => u.Languages)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "Login");

            // Load all governorates for dropdown
            var governorates = _context.Governorates.ToList();
            ViewBag.Governorates = new SelectList(governorates, "GovernorateId", "Name");

            // If user has residence with governorate selected, load districts
            if (user.Residences?.FirstOrDefault()?.GovernorateId != null)
            {
                int governorateId = user.Residences.First().GovernorateId.Value;
                var districts = _context.Districts.Where(d => d.GovernorateId == governorateId).ToList();
                ViewBag.Districts = new SelectList(districts, "DistrictId", "Name");

                // If district selected, load villages
                if (user.Residences.First().DistrictId != null)
                {
                    int districtId = user.Residences.First().DistrictId.Value;
                    var villages = _context.Villages.Where(v => v.DistrictId == districtId).ToList();
                    ViewBag.Villages = new SelectList(villages, "VillageId", "Name");
                }
            }
            else
            {
                ViewBag.Districts = new SelectList(new List<District>(), "DistrictId", "Name");
                ViewBag.Villages = new SelectList(new List<Village>(), "VillageId", "Name");
            }

            ViewBag.EducationLevels = new SelectList(new List<string> {
                "ثانوي", "دبلوم", "بكالوريوس", "ماجستير", "دكتوراه"
            });

            ViewBag.ProgramTypes = new SelectList(new List<string> {
                "تخصص أكاديمي", "تدريب مهني", "تطوير ذاتي"
            });

            ViewBag.Languages = new SelectList(new List<string> {
                "العربية", "الإنجليزية", "الفرنسية", "الألمانية", "الإسبانية", "أخرى"
            });

            ViewBag.ProficiencyLevels = new SelectList(new List<string> {
                "اللغة الأم", "طلاقة", "متقدم", "متوسط", "مبتدئ"
            });

            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User updatedUser,
            int? residenceGovernorateId, int? residenceDistrictId, int? residenceVillageId,
            string residenceCustomVillage, bool isCustomVillage,
            string educationLevel, string programType,
            List<string> languageNames, List<string> customLanguageNames, List<string> proficiencyLevels, IFormFile? profileImage)
        {
            var existingUser = _context.Users
                .Include(u => u.Residences)
                .Include(u => u.Educations)
                .Include(u => u.Languages)
                .FirstOrDefault(u => u.Id == updatedUser.Id);

            if (existingUser == null) return NotFound();

            // Update basic user information
            existingUser.FirstName = updatedUser.FirstName;
            existingUser.MiddleName = updatedUser.MiddleName;
            existingUser.LastName = updatedUser.LastName;
            existingUser.Gender = updatedUser.Gender;
            existingUser.Birthdate = updatedUser.Birthdate;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;


            // تحديث صورة الملف الشخصي
            if (profileImage != null && profileImage.Length > 0)
            {
                // حفظ الصورة في المسار المحدد
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", profileImage.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                // تحديث مسار الصورة في قاعدة البيانات
                existingUser.ProfileImagePath = "/images/" + profileImage.FileName;

                HttpContext.Session.SetString("ProfileImagePath", existingUser.ProfileImagePath);


            }


            // مثال في Controller بعد تسجيل الدخول



            // Update residence information
            UpdateResidence(existingUser, residenceGovernorateId, residenceDistrictId, residenceVillageId, residenceCustomVillage, isCustomVillage);

            // Update education information
            UpdateEducation(existingUser, educationLevel, programType);

            // Update languages
            UpdateLanguages(existingUser, languageNames, customLanguageNames, proficiencyLevels);

            _context.SaveChanges();
            TempData["Success"] = "تم تحديث البيانات بنجاح";
            return RedirectToAction("Edit");
        }

        [HttpGet]
        public IActionResult GetDistricts(int governorateId)
        {
            var districts = _context.Districts
                .Where(d => d.GovernorateId == governorateId)
                .Select(d => new { d.DistrictId, d.Name })
                .ToList();

            return Json(districts);
        }

        [HttpGet]
        public IActionResult GetVillages(int districtId)
        {
            var villages = _context.Villages
                .Where(v => v.DistrictId == districtId)
                .Select(v => new { v.VillageId, v.Name })
                .ToList();

            return Json(villages);
        }
        private void UpdateResidence(User user, int? governorateId, int? districtId, int? villageId, string residenceCustomVillage, bool isCustomVillage)
        {
            var residence = user.Residences?.FirstOrDefault();

            if (residence == null)
            {
                residence = new Residence
                {
                    UserId = user.Id,
                    GovernorateId = governorateId,
                    DistrictId = districtId,
                    VillageId = null, // دائمًا null
                    IsCustomVillage = true,
                    Village = !string.IsNullOrEmpty(residenceCustomVillage) ? residenceCustomVillage : ""
                };

                residence.Governorate = governorateId.HasValue
                    ? _context.Governorates.Find(governorateId.Value)?.Name ?? ""
                    : "";

                residence.District = districtId.HasValue
                    ? _context.Districts.Find(districtId.Value)?.Name ?? ""
                    : "";

                user.Residences = new List<Residence> { residence };
                _context.Residences.Add(residence);
            }
            else
            {
                residence.GovernorateId = governorateId;
                residence.DistrictId = districtId;
                residence.VillageId = null; // دائمًا null
                residence.IsCustomVillage = true;
                residence.Village = !string.IsNullOrEmpty(residenceCustomVillage) ? residenceCustomVillage : "";

                residence.Governorate = governorateId.HasValue
                    ? _context.Governorates.Find(governorateId.Value)?.Name ?? ""
                    : "";

                residence.District = districtId.HasValue
                    ? _context.Districts.Find(districtId.Value)?.Name ?? ""
                    : "";
            }
        }



        private void UpdateEducation(User user, string educationLevel, string programType)
        {
            var education = user.Educations?.FirstOrDefault();

            if (education == null)
            {
                // Create new education record if it doesn't exist
                education = new Education
                {
                    UserId = user.Id,
                    EducationLevel = educationLevel,
                    ProgramType = programType
                };

                user.Educations = new List<Education> { education };
                _context.Educations.Add(education);
            }
            else
            {
                // Update existing education
                education.EducationLevel = educationLevel;
                education.ProgramType = programType;
            }
        }

        private void UpdateLanguages(User user, List<string> languageNames, List<string> customLanguageNames, List<string> proficiencyLevels)
        {
            // Remove all existing languages
            if (user.Languages != null && user.Languages.Any())
            {
                _context.Languages.RemoveRange(user.Languages);
            }

            // Add new languages
            if (languageNames != null && languageNames.Any())
            {
                user.Languages = new List<Language>();

                for (int i = 0; i < languageNames.Count; i++)
                {
                    string customName = null;
                    if (languageNames[i] == "أخرى" && customLanguageNames != null && i < customLanguageNames.Count)
                    {
                        customName = customLanguageNames[i];
                    }

                    var language = new Language
                    {
                        UserId = user.Id,
                        LanguageName = languageNames[i],
                        CustomLanguageName = customName,
                        ProficiencyLevel = i < proficiencyLevels.Count ? proficiencyLevels[i] : "متوسط"
                    };

                    user.Languages.Add(language);
                    _context.Languages.Add(language);
                }
            }




        }

        public async Task<IActionResult> registerationWorkshops()
        {
            // جيب الـ UserId من الـ Claims
            var userId = HttpContext.Session.GetInt32("UserId");

            var userRegistrations = await _context.WorkshopRegistrations
                .Include(wr => wr.Workshop)
                .Where(wr => wr.UserId == userId)
                .OrderByDescending(wr => wr.RegisteredAt)
                .ToListAsync();

            return View(userRegistrations);
        }

        // إلغاء التسجيل من ورشة
        //[HttpPost]
        //public async Task<IActionResult> Unregister(int registrationId)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    var registration = await _context.WorkshopRegistrations
        //        .FirstOrDefaultAsync(wr => wr.Id == registrationId && wr.UserId == userId);

        //    if (registration != null)
        //    {
        //        _context.WorkshopRegistrations.Remove(registration);
        //        await _context.SaveChangesAsync();
        //        TempData["Success"] = "تم إلغاء التسجيل بنجاح";
        //    }
        //    else
        //    {
        //        TempData["Error"] = "حدث خطأ في إلغاء التسجيل";
        //    }

        //    return RedirectToAction("registerationWorkshops");
        //}


    }
}