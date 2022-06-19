using System.ComponentModel.DataAnnotations;
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Models
{
    public class TextFilter
    {
        public int Id { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "Message must be longer then 30 characters")]
        public string Text { get; set; }
        public string Comment { get; set; }
        public TypeOfFilter Filter { get; set; }
    }
}
