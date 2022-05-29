using System;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.Chats
{
    public class ChatMessage
    {
        private readonly ChatUser _user;
        private readonly string _text;
        private readonly Message _message;
        public ChatMessage(Update update)
        {
            if (update is null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            _user = new ChatUser(update);
            _text = update?.Message?.Text;
            _message = update?.Message;
        }

        public ChatUser GetCurrentUser() => _user;
        public Message GetCurrentMessage() => _message;
        public string GetCurrentMessageText() => _text ?? "";
    }
}
