using System.Text.RegularExpressions;

namespace TelBotApplication.Domain.Dtos
{
    public class AdminDTO
    {
        public string UserName { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
    }
}
