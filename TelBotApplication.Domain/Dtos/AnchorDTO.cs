using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Dtos
{
    public class AnchorDTO
    {
        public string Tag { get; set; }
        public string Message { get; set; }
        public AnchorCallBack TriggerCallBack { get; set; }
        public TypeOfFilter Filter { get; set; }
        public AnchorAction AnchorAction { get; set; }
    }
}
