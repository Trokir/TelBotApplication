using System;
using System.Threading.Tasks;

namespace TelBotApplication.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IBotCommandService BotCommandService { get; }
        public IVenueCommandService VenueCommandServise { get; }
        public IAdminService AdminService { get; }
        public IMessageLoggerService MessageLoggerService { get; }
        public IGroupService GroupService { get; }
        public ITextFilterService TextFilterService { get; }
        Task<int> Complete();
    }
}
