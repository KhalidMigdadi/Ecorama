namespace Ecorama.Models
{
    public class UserExportViewModel
    {
        // بيانات المستخدم الأساسية
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public string NationalId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // السكن
        public string Governorate { get; set; }
        public string District { get; set; }
        public string Village { get; set; }

        // التعليم
        public string EducationLevel { get; set; }
        public string ProgramType { get; set; }

        // اللغة
        public string LanguageName { get; set; }
        public string CustomLanguageName { get; set; }
        public string ProficiencyLevel { get; set; }
    }

}
