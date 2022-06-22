
namespace TelBotApplication.Domain.Models.Anchors
{
    public class AnchorCallback
    {
        public int Id { get; set; }
       
        public  string ButtonText { get; set; }
        public string ButtonCondition { get; set; }
        public int AnchorId { get; set; }
        public Anchor Anchor { get; set; }
    }
}
