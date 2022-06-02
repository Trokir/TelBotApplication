using System;
using System.Threading.Tasks;
using TelBotApplication.DAL.Services;

namespace TelBotApplication.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IBotCommandService BotCommandService { get; }
        IVenueCommandServise VenueCommandServise { get; }
        Task<int> Complete();
    }
}
