namespace TelBotApplication.Domain.Interfaces
{
    public interface ISpamConfiguration
    {
        string SpamTextMessagePath { get; }
        string UrlSpamClassification { get; }
    }
}