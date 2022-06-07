namespace TelBotApplication.Filters
{
    public interface IFludFilter
    {
        bool CheckIsSpamOrHam(string value);
        bool CheckIsSpam(string value);
    }
}