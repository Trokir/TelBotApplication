using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
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


        public void AddNewMember(Message message, DateTime addDate)
        {
            var member = new Member { Id = message.From.Id, Message = message, AddDate = addDate };
            if (_members.TryAdd(member.Id, member)) return;
            
        }

        public void DropNewMember(long id)
        {
            if (_members.TryGetValue(id, out var value))
            {
                if (_members.TryRemove(id, out value))
                {
                    return;
                }
                DropNewMember(id);
            }
        }
        public void ClearMembersList()
        {
            _members.Clear();
        }

        public async Task RunAlertPolling()
        {
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (var member in _members)
                    {
                        var diffInSeconds = (DateTime.Now - member.Value.AddDate).TotalSeconds;
                        if (diffInSeconds >= 15 && diffInSeconds < 17 && !member.Value.IsAlerted && !member.Value.IsRestricted)
                        {
                            await AlertEvent?.Invoke(member.Value.Message);
                            member.Value.IsAlerted = true;
                        }
                        else if (diffInSeconds >= 30 && diffInSeconds < 32 && member.Value.IsAlerted && !member.Value.IsRestricted)
                        {
                            RestrictEvent?.Invoke(member.Value.Message);
                            member.Value.IsRestricted = true;
                        }
                    }
                }
            });
        }



    }
}