using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static TelBotApplication.Domain.Chats.MemberExecutor;

namespace TelBotApplication.Domain.Chats
{
    public interface IMemberExecutor
    {
        event AlertEventHandler AlertEvent;
        event RestrictEventHandler RestrictEvent;

        void AddNewMember(Message message, DateTime addDate);
        void DropNewMember(long id);
        void ClearMembersList();
        Task RunAlertPolling();
    }
}
