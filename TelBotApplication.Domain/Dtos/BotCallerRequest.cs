
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Dtos
{
    public class BotCallerRequest
    {
        public string Link { get; set; }
        public string Command { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public TypeOfreactions TypeOfreaction { get; set; }
    }
}
