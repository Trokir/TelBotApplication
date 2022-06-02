using System.ComponentModel.DataAnnotations;
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        [Required]

        [MinLength(30, ErrorMessage = "Message must be longer then 30 characters")]

        public string Message { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }
    }
}
