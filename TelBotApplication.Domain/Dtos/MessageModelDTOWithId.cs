using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Dtos
{
    public class MessageModelDTOWithId
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Message { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }
    }

}
