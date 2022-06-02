using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Domain.Interfaces;
using Telegram.Bot.Types;

namespace TelBotApplication.Domain.NewFolder.Executors
{
    public class MemberExecutor : IMemberExecutor
    {

        private readonly BlockingCollection<Member> _members;
        public delegate Task AlertEventHandler(Message message, CancellationToken cancellationToken = default);
        public event AlertEventHandler AlertEvent;
        public delegate Task RestrictEventHandler(Message message, CancellationToken cancellationToken = default);
        public event RestrictEventHandler RestrictEvent;


        public MemberExecutor()
        {
            _members = new BlockingCollection<Member>();
        }


        public void AddNewMember(Message message, DateTime addDate)
        {
            if (!_members.Any(x => x.Message == message))
            {
                bool result = _members.TryAdd(new Member { Message = message, AddDate = addDate });
                if (result)
                {
                    return;
                }
            }
        }

        public void DropNewMember(Message message)
        {
            if (_members.Any(x => x.Message.MessageId == message.MessageId))
            {
                Member member = _members.FirstOrDefault(x => x.Message.MessageId == message.MessageId);
                RemoveMember(_members, member);
                Thread.Sleep(200);
            }
        }
        private bool RemoveMember(BlockingCollection<Member> self, Member itemToRemove)
        {
            lock (self)
            {
                Member comparedItem;
                List<Member> itemsList = new List<Member>();
                do
                {
                    bool result = self.TryTake(out comparedItem);
                    if (!result)
                    {
                        return false;
                    }

                    if (!comparedItem.Equals(itemToRemove))
                    {
                        itemsList.Add(comparedItem);
                    }
                } while (!comparedItem.Equals(itemToRemove));
                Parallel.ForEach(itemsList, t => self.Add(t));
            }
            return true;
        }


        public async Task RunAlertPolling()
        {
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (Member member in _members.ToList())
                    {
                        double diffInSeconds = (DateTime.Now - member.AddDate).TotalSeconds;
                        if (diffInSeconds >= 15 && diffInSeconds < 17 && !member.IsAlerted && !member.IsRestricted)
                        {
                            await AlertEvent?.Invoke(member.Message);
                            member.IsAlerted = true;
                        }
                        else if (diffInSeconds >= 30 && diffInSeconds < 32 && member.IsAlerted && !member.IsRestricted)
                        {
                            RestrictEvent?.Invoke(member.Message);
                            member.IsRestricted = true;
                        }
                    }
                }
            });
        }



    }
}
