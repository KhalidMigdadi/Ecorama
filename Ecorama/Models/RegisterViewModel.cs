using System.ComponentModel.DataAnnotations;

namespace Ecorama.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; }

        [Display(Name = "الاسم الأوسط")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [Display(Name = "الاسم الأخير")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور على الأقل {2} أحرف", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمات المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^(07[7-9]|079)\d{7}$", ErrorMessage = "يرجى إدخال رقم هاتف أردني صحيح")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        public DateTime Birthdate { get; set; }

        [Required(ErrorMessage = "الرقم الوطني مطلوب")]
        [StringLength(10, ErrorMessage = "الرقم الوطني يجب أن يكون 10 أرقام", MinimumLength = 10)]
        [Display(Name = "الرقم الوطني")]
        public string NationalId { get; set; }

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [Display(Name = "المحافظة")]
        public string Governorate { get; set; } = "إربد";

        [Required(ErrorMessage = "اللواء مطلوب")]
        [Display(Name = "اللواء")]
        public string District { get; set; }

        [Required(ErrorMessage = "البلدة / القرية مطلوبة")]
        [Display(Name = "البلدة / القرية")]
        public string Village { get; set; }

        [Required(ErrorMessage = "مستوى التعليم مطلوب")]
        [Display(Name = "مستوى التعليم")]
        public string EducationLevel { get; set; }

        [Required(ErrorMessage = "نوع البرنامج مطلوب")]
        [Display(Name = "نوع البرنامج")]
        public string ProgramType { get; set; }

        public string Nationality { get; set; }


        public List<LanguageViewModel> Languages { get; set; } = new List<LanguageViewModel>();



    }
}
