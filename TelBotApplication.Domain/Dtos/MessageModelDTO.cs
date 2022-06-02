using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Dtos
{

    public class MessageModelDTO
    {
        public string Message { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }
    }
}
