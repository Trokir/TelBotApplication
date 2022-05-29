using System.ComponentModel.DataAnnotations;
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Models
{
    public class BotCaller
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(24)]
        [MinLength(4)]
        [RegularExpression(@"/^\//", ErrorMessage = "First character must be '/'")]
        public string Command { get; set; }
        [RegularExpression(@"^(http[s]?:\\/\\/(www\\.)?|ftp:\\/\\/(www\\.)?
                            |www\\.){1}([0-9A-Za-z-\\.@:%_\+~#=]+)+((\\.
                            [a-zA-Z]{2,3})+)(/(.)*)?(\\?(.)*)?", ErrorMessage = "The link is not valid format")]
        public string Link { get; set; }

        [MaxLength(1000)]
        public string Caption { get; set; }

        [Required]
        [MaxLength(24)]
        [MinLength(4)]
        public string Description { get; set; } = "none";

        [Required]
        public TypeOfreactions TypeOfreaction { get; set; }
    }
}
