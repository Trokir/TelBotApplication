namespace TelBotApplication.Filters
{
    public interface IFludFilter
    {

        bool CheckIsSpam(string value);
    }
}