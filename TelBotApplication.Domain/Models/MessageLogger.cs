using System;
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Models
{
    public class MessageLogger
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public long ChatId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime AddedDate { get; set; }
        public TypeOfMessageLog TypeOfMessageLog { get; set; }
        public virtual Group Group { get; set; }
    }
}
