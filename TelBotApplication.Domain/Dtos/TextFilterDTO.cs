
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Dtos
{
    public class TextFilterDTO
    {
        public string Text { get; set; }
        public TypeOfFilter Filter { get; set; }
        public string Comment { get; set; }
    }
}
