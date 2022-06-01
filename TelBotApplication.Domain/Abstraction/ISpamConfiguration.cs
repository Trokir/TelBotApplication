namespace TelBotApplication.Domain.Abstraction
{
    public interface ISpamConfiguration
    {
        string SpamTextMessagePath { get; }
        string UrlSpamClassification { get; }
    }
}