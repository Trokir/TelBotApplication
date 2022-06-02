namespace TelBotApplication.Domain.Dtos
{
    public class VenueRequestUpdate
    {
        public int Id { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string Command { get; set; }
    }
}
