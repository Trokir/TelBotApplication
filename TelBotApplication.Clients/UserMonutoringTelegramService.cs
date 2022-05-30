
using GSF.FuzzyStrings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Helpers;
using TL;

namespace TelBotApplication.Clients
{
    public class UserMonutoringTelegramService : BackgroundService
    {
        private readonly EnvironmentConfiguration _environmentConfiguration;
        public readonly WTelegram.Client Client;
        private readonly IConfiguration _config;
        public User User { get; private set; }
        public Task<string> ConfigNeeded() => _configNeeded.Task;
        private TaskCompletionSource<string> _configNeeded = new();
        private readonly ManualResetEventSlim _configRequest = new();
        private string _configValue;
        private ChatBase _chatBase;

        private readonly ILogger<UserMonutoringTelegramService> _logger;
        public UserMonutoringTelegramService(IConfiguration config,
            IOptions<EnvironmentConfiguration> options,
            ILogger<UserMonutoringTelegramService> logger
            )
        {
            _config = config;
            _logger = logger;
            _environmentConfiguration = options.Value;
            WTelegram.Helpers.Log = (lvl, msg) => logger.Log((LogLevel)lvl, msg);
            Client = new WTelegram.Client(Config);
        }
        private string Config(string what)
        {
            switch (what)
            {
                case "verification_code":
                case "password":
                    _configNeeded.SetResult(what);
                    _configRequest.Wait();
                    _configRequest.Reset();
                    return _configValue;
                case "api_id":
                    return _environmentConfiguration.api_id;
                case "api_hash":
                    return _environmentConfiguration.api_hash;
                case "phone_number":
                    return _environmentConfiguration.phone_number;
                case "first_name":
                    return _environmentConfiguration.first_name;
                case "last_name":
                    return _environmentConfiguration.last_name;
                default:
                    var res = _config[what];
                    return res; // use the ASP.NET configuration (see launchSettings.json)
            }
        }
        public void ReplyConfig(string value)
        {
            _configValue = value;
            _configNeeded = new();
            _configRequest.Set();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                User = await Client.LoginUserIfNeeded();
                var chats = await Client.Messages_GetAllChats();
                _chatBase = chats.chats[1238311479];
               // _chatBase = chats.chats[1640302974];
               
                await ListenUpdate();
            }
            catch (Exception ex)
            {
                _configNeeded.SetException(ex);
                throw;
            }
            _configNeeded.SetResult(null); // login complete
        }

        private async Task ListenUpdate()
        {
            Client.Update += Client_Update;
        }


        private void Client_Update(IObject arg)
        {
            if (arg is not UpdatesBase notupdates) return;
            if (arg is UpdatesBase updates)
            {
                foreach (var update in updates.UpdateList)
                    switch (update)
                    {
                        case UpdateNewMessage unm: DisplayMessage(unm.message); break;
                        case UpdateEditMessage uem: DisplayMessage(uem.message, true); break;
                        // case UpdateDeleteChannelMessages udcm: Console.WriteLine($"{udcm.messages.Length} message(s) deleted in {Chat(udcm.channel_id)}"); break;
                       // case UpdateDeleteMessages udm: Console.WriteLine($"{udm.messages.Length} message(s) deleted"); break;
                        //case UpdateUserTyping uut: Console.WriteLine($"{CurrentUser(uut.user_id)} is {uut.action}"); break;
                        //case UpdateChatUserTyping ucut: Console.WriteLine($"{Peer(ucut.from_id)} is {ucut.action} in {Chat(ucut.chat_id)}"); break;
                        //case UpdateChannelUserTyping ucut2: Console.WriteLine($"{Peer(ucut2.from_id)} is {ucut2.action} in {Chat(ucut2.channel_id)}"); break;
                        //case UpdateChatParticipants { participants: ChatParticipants cp }: Console.WriteLine($"{cp.participants.Length} participants in {Chat(cp.chat_id)}"); break;
                        //case UpdateUserStatus uus: Console.WriteLine($"{CurrentUser(uus.user_id)} is now {uus.status.GetType().Name[10..]}"); break;
                        //case UpdateUserName uun: Console.WriteLine($"{CurrentUser(uun.user_id)} has changed profile name: @{uun.username} {uun.first_name} {uun.last_name}"); break;
                        //case UpdateUserPhoto uup: Console.WriteLine($"{CurrentUser(uup.user_id)} has changed profile photo"); break;
                        default: Console.WriteLine(""); break; // there are much more update types than the above cases
                    }
            }


        }

        private async void DisplayMessage(MessageBase messageBase, bool edit = false)

        {
            if (edit) Console.Write("(Edit): ");
            switch (messageBase)
            {
                case Message m:
                    var vv = m.message.ToLower();
                    bool total = false;
                    var keywords = new string[] { "отчество", "выйграл" };

                    if (long.TryParse(m.peer_id.ID.ToString(), out var chatIt))
                    {
                        Regex rules_name_nobadlang = new Regex(@"[б6b]+[\s\S]*[лl]+[\s\S]*[я(?:ya)(?:йа)]+
                                       |[xхh]+[\s\S]*[уuy]+[\s\S]*[йиin1u]+|[пnpр]+[\s\S]*[eеэйиi1nu]+[\s\S]*[дd]+[\s\S]*
                                       [рpr]+|[xхh]+[\s\S]*[eэе]+[\s\S]*[рpr]+|[пnpр]+[\s\S]*[eэейиn1iu]
                                       +[\s\S]*[3зz]+[\s\S]*[дd]+|[ш(?:sh)щ]+[\s\S]*[лl]+[\s\S]*
                                       [юуuy]+[\s\S]*[xхh]+|[eеэ]+[\s\S]*[б6b]+[\s\S]*[аaиi1nu]+");


                        if (chatIt == _chatBase.ID && !string.IsNullOrEmpty(vv))
                        {
                            //if (rules_name_nobadlang.IsMatch(vv))
                            //{
                            //    await Client.SendMessageAsync(_chatBase, $"мат");
                            //}

                            var result1 = vv.ApproximatelyEquals(keywords[0], FuzzyStringComparisonOptions.UseLongestCommonSubstring, FuzzyStringComparisonTolerance.Strong);
                            var result2 = vv.ApproximatelyEquals(keywords[0], FuzzyStringComparisonOptions.UseLongestCommonSubsequence, FuzzyStringComparisonTolerance.Strong);
                            var result3 = vv.ApproximatelyEquals(keywords[0], FuzzyStringComparisonOptions.UseLevenshteinDistance, FuzzyStringComparisonTolerance.Normal);

                            if (total.TotalApproximatelyEquals(new bool[] { result1, result2/*, result3*/ }))
                            {
                                await Client.SendMessageAsync(_chatBase, $"Запретное слово", reply_to_msg_id: m.id);
                            }
                            result1 = vv.ApproximatelyEquals(keywords[1], FuzzyStringComparisonOptions.UseLongestCommonSubstring, FuzzyStringComparisonTolerance.Strong);
                            result2 = vv.ApproximatelyEquals(keywords[1], FuzzyStringComparisonOptions.UseLongestCommonSubsequence, FuzzyStringComparisonTolerance.Strong);
                             result3 = vv.ApproximatelyEquals(keywords[1], FuzzyStringComparisonOptions.UseLevenshteinDistance, FuzzyStringComparisonTolerance.Normal);

                            if (total.TotalApproximatelyEquals(new bool[] { result1, result2/*, result3 */}))
                            {

                                await Client.SendMessageAsync(_chatBase, $"/badword {m.id}");
                            }
                        }

                    }

                    break;

            }
        }


    }
}
