namespace TelBotApplication.Domain.Dtos
{
    public class GroupRequestForUpdate
    {
        public virtual int Id { get; set; }
        public virtual long ChatId { get; set; }
        public string Title { get; set; }
    }
}
