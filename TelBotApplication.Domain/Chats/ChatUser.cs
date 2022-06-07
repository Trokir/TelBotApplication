using System;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.Chats
{
    public class ChatUser
    {
        private readonly string _lastName;
        private readonly string _firstName;
        private readonly int _messageId;
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
            _messageId = message?.MessageId ?? -1;
        }
        public string UserName { get => _userName; }
        public int MessageId { get => _messageId; }
        public long UserId { get => _userId; }
        public string FirstName { get => _firstName; }
        public bool IsBot { get => _isBot; }
        public string FullName { get => $"{_firstName} {_lastName}"; }
    }
}
