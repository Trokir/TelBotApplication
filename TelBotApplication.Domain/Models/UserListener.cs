using System.ComponentModel.DataAnnotations;

namespace TelBotApplication.Domain.Models
{
    public class UserListener
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        [MinLength(4)]

        public string ApiId { get; set; }
        [Required]

        public string ApiHash { get; set; }

        [Required(ErrorMessage = "Mobile no. is required")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string PhoneNumber { get; set; }
        [Required]
        [MaxLength(30)]
        [MinLength(4)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(30)]
        [MinLength(4)]
        public string LastName { get; set; }
    }
}
