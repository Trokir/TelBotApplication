using System.Threading;
using System.Threading.Tasks;

namespace TelBotApplication.Clients
{
    public interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
}
