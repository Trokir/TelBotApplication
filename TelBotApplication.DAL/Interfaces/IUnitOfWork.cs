using System;
using System.Threading.Tasks;

namespace TelBotApplication.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBotCommandService BotCommandService { get; }
        IVenueCommandServise VenueCommandServise { get; }
        Task<int> Complete();
    }
}
