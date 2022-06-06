namespace TelBotApplication.Domain.Enums
{
    public enum TypeOfreactions
    {
        Text = 1,
        Animation = 2,
        Venue = 3,
        Photo = 4
    }
    public enum TypeOfMessage
    {
        Spam = 1,
        Ham = 2
    }
    public enum TypeOfMessageLog
    {
        Added,
        Edited,
        Deleted
    }
}
