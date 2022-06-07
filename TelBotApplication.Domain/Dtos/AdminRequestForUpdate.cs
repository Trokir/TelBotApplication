using System.Text.RegularExpressions;

namespace TelBotApplication.Domain.Dtos
{
    public class AdminRequestForUpdate
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
    }
}
