using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static TelBotApplication.Domain.NewFolder.Executors.MemberExecutor;

namespace TelBotApplication.Domain.Interfaces
{
    public interface IMemberExecutor
    {
        event AlertEventHandler AlertEvent;
        event RestrictEventHandler RestrictEvent;

        void AddNewMember(Message message, DateTime addDate);
        void DropNewMember(Message message);
        Task RunAlertPolling();
    }
}