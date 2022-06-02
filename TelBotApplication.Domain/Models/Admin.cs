

namespace TelBotApplication.Domain.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
    }
}
