using System;
using Telegram.Bot.Types;
using static TelBotApplication.Domain.Chats.MemberExecutor;

namespace TelBotApplication.Domain.Chats
{
    public interface IMemberExecutor
    {
        event AlertEventHandler AlertEvent;
        event RestrictEventHandler RestrictEvent;

        void AddNewMember(CallBackUser user, Message message, DateTime addDate);
        void DropNewMember(CallbackQuery callbackQuery);
        void RunAlertPolling();
    }
}
