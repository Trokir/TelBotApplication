using System.Collections.Generic;
using TelBotApplication.Domain.Models.Anchors;

namespace TelBotApplication.Domain.Models
{
    public class Group
    {
        public Group()
        {
            Admins = new HashSet<Admin>();
            MessageLoggers = new HashSet<MessageLogger>();
            Anchors = new HashSet<Anchor>();
        }
        public virtual int Id { get; set; }
        public virtual long ChatId { get; set; }

        public ICollection<Admin> Admins;
        public ICollection<MessageLogger> MessageLoggers;
        public ICollection<Anchor> Anchors { get; set; }
    }
}
