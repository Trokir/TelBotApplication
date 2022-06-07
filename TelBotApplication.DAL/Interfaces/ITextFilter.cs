namespace TelBotApplication.DAL.Interfaces
{
    public interface ITextFilter
    {
        bool IsAlertFrase(string text);
    }
}