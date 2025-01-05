using System.ComponentModel.DataAnnotations;

namespace savnmore.Models
{
    public class ContactForm
    {

        [Required]
        [Display(Name = "Your Name")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [RegularExpression(Constants.EmailRegex, ErrorMessage = "Not a vlid email address.")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Your Message")]
        public string Message { get; set; }
    }
}