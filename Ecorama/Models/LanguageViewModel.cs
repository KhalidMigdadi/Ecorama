using System.ComponentModel.DataAnnotations;

namespace Ecorama.Models
{
    public class LanguageViewModel
    {

        [Required(ErrorMessage = "اسم اللغة مطلوب")]
        public string LanguageName { get; set; }

        public string CustomLanguageName { get; set; }

        [Required(ErrorMessage = "مستوى الإتقان مطلوب")]
        public string ProficiencyLevel { get; set; }
    }
}
