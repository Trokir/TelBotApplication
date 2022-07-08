using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.Chats
{
    public class MemberExecutor : IMemberExecutor
    {

        private readonly ConcurrentDictionary<long, Member> _members;
        public delegate Task AlertEventHandler(Message message, CancellationToken cancellationToken = default);
        public event AlertEventHandler AlertEvent;
        public delegate Task RestrictEventHandler(Message message, CancellationToken cancellationToken = default);
        public event RestrictEventHandler RestrictEvent;


        public MemberExecutor()
        {
            _members = new ConcurrentDictionary<long, Member>();
        }


        public void AddNewMember(CallBackUser user,Message message, DateTime addDate)
        {
            var member = new Member { Id = user.UserId, Message = message, AddDate = addDate };
            if (_members.TryAdd(member.Id, member)) return;

        }

        public void DropNewMember(CallbackQuery callbackQuery)
        {
            if (_members.TryGetValue(callbackQuery.From.Id, out var value))
            {
                if (_members.TryRemove(callbackQuery.From.Id, out value))
                {
                    return;
                }
            }
        }
        public void ClearMembersList()
        {
            _members.Clear();
        }

        public void RunAlertPolling()
        {

            while (true)
            {
                foreach (var member in _members)
                {
                    var diffInSeconds = (DateTime.Now - member.Value.AddDate).TotalSeconds;
                    if (diffInSeconds >= 30 && diffInSeconds < 32 && !member.Value.IsAlerted && !member.Value.IsRestricted)
                    {

                        AlertEvent?.Invoke(member.Value.Message);
                        member.Value.IsAlerted = true;

                    }
                    else if (diffInSeconds >= 60 && diffInSeconds < 62 && member.Value.IsAlerted && !member.Value.IsRestricted)
                    {
                        RestrictEvent?.Invoke(member.Value.Message);
                        member.Value.IsRestricted = true;

                    }
                }
            }

        }



    }
}