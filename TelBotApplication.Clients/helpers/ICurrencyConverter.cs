using System.Threading.Tasks;

namespace TelBotApplication.Clients.helpers
{
    public interface ICurrencyConverter
    {
        Task<decimal> GetCurrencyFromEUR(float amount);
        Task<decimal> GetCurrencyFromRubles(float amount);
        Task<decimal> GetCurrencyFromUSD(float amount);
        Task<decimal> GetCurrencyFromReal(float amount);
    }
}