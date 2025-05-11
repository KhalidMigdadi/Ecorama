using System.ComponentModel.DataAnnotations;

namespace Ecorama.Models
{
    public class RegisterViewModel
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

     
        public string LastName { get; set; }


        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; }


        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        public string Gender { get; set; }

   
        public DateTime Birthdate { get; set; }


        public string NationalId { get; set; }

  
        public string Governorate { get; set; }

        public string District { get; set; }

 
        public string Village { get; set; }

        public string? CustomVillage { get; set; }


        public string EducationLevel { get; set; }

        public string ProgramType { get; set; }


        public List<LanguageViewModel>? Languages { get; set; } = new List<LanguageViewModel>();


    }
}
