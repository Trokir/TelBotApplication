namespace TelBotApplication.Domain.Models
{
    public class VenueCommand
    {
        public int Id { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string Command { get; set; }

    }
}
