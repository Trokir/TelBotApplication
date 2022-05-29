using System.Linq;

namespace TelBotApplication.Domain.Helpers
{
    public static class FuzzyStringComparisonUtil
    {
        public static bool TotalApproximatelyEquals(this bool value, params bool[] list)
        {
            if (list.Any())
            {
                return list.All(x => x == true);
            }
            return false;
        }
    }
}
