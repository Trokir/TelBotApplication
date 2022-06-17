using System;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.Chats
{

    public class Member
    {
        public long Id { get; set; }
        public Message Message { get; set; }
        public DateTime AddDate { get; set; }
        public bool IsAlerted { get; set; }
        public bool IsRestricted { get; set; }
    }
}
