using System.Collections.Generic;

namespace TelBotApplication.Domain.Models
{
    public class Group
    {
        public Group()
        {
            Admins = new HashSet<Admin>();
            MessageLoggers = new HashSet<MessageLogger>();
        }
        public virtual int Id { get; set; }
        public virtual long ChatId { get; set; }

        public ICollection<Admin> Admins;
        public ICollection<MessageLogger> MessageLoggers;

    }
}
