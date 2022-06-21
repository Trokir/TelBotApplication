using System.ComponentModel.DataAnnotations;
using TelBotApplication.Domain.Enums;

namespace TelBotApplication.Domain.Models.Anchors
{
    public class Anchor
    {
        public int Id { get; set; }
        public long GroupId { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Message must be longer then 30 characters")]
        public string Tag { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Message must be longer then 30 characters")]
        public string Message { get; set; }
        public AnchorCallBack TriggerCallBack { get; set; }
        public TypeOfFilter Filter { get; set; }
        public AnchorAction AnchorAction { get; set; }
        public virtual Group Group { get; set; }
    }
}
