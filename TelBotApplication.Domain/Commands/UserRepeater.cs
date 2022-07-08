

namespace TelBotApplication.Domain.Commands
{
    public class UserRepeater
    {
        public long UserId { get; set; }
        public int ReplyToMessageId { get; set; }
    }
}
