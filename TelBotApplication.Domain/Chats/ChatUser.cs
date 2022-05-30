using System;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.Chats
{
    public class ChatUser
    {
        private readonly string _lastName;
        private readonly string _firstName;
        private readonly string _userName;
        private readonly long _userId;
        private readonly bool _isBot;
        public ChatUser(Update update)
        {
            if (update is null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            Message message = update.Message;
            _lastName = message?.From?.LastName ?? "no name";
            _firstName = message?.From?.FirstName ?? "";
            _userName = message?.From?.Username ?? string.Empty;
            _userId = message?.From.Id ?? -1;
            _isBot = message?.From.IsBot ?? false;
        }
        public string UserName { get => _userName; }
        public long GetUserId() => _userId;
        public bool CheckIsBot() => _isBot;
        public string GetFullName() => $"{_firstName} {_lastName}";
    }
}
