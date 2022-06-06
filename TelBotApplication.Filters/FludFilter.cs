namespace TelBotApplication.Filters
{
    public class FludFilter : IFludFilter
    {


        public bool CheckIsSpam(string value)
        {
            return false;
        }

        public bool CheckIsSpamOrHam(string value)
        {
            return false;
        }

        public bool IsFacebookSpam(string value)
        {
            return false;
        }

    }
}
