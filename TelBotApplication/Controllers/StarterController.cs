using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TelBotApplication.Clients;
using TL;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StarterController : ControllerBase
    {
        private readonly UserMonutoringTelegramService _userMonutoringService;
        private readonly ILogger<StarterController> _logger;
        private Messages_Chats _chats;
        public StarterController(UserMonutoringTelegramService userMonutoringService,
            ILogger<StarterController> logger)
        {
            _userMonutoringService = userMonutoringService;
            _logger = logger;
            if (_userMonutoringService.User != null)
            {
                _chats = _userMonutoringService.Client.Messages_GetAllChats(null).Result;

            }
        }

        [HttpGet("config")]
        public ActionResult Config(string value)
        {
            _userMonutoringService.ReplyConfig(value);
            return Redirect("status");
        }

        [HttpGet("status")]
        public async Task<ContentResult> Status()
        {
            var config = await _userMonutoringService.ConfigNeeded();
            if (config != null)
                return Content($@"Enter {config}: <form action=""config""><input name=""value"" autofocus/></form>", "text/html");
            else
            {
                _chats = await _userMonutoringService.Client.Messages_GetAllChats(null);
                return Content($@"Connected as {_userMonutoringService.User}<br/><a href=""chats"">Get all chats</a>", "text/html");

            }
        }
        [HttpGet("chats")]
        public async Task<object> Chats()
        {
            if (_userMonutoringService.User == null) throw new Exception("Complete the login first");
            var chats = await _userMonutoringService.Client.Messages_GetAllChats(null);
            return chats.chats;
        }
        [HttpGet("chatbyId")]
        public async Task<object> ChatById(long id = 1238311479)
        {
            if (_userMonutoringService.User == null) throw new Exception("Complete the login first");
            var chats = await _userMonutoringService.Client.Messages_GetAllChats();
            var state = await _userMonutoringService.Client.Updates_GetState();


            var upd = await _userMonutoringService.Client.Updates_GetDifference(state.pts - 1000, date: DateTime.Now, state.qts - 100);
            var mess = upd.NewMessages;
            var chat = chats.chats[id]; // the chat we want
            var full = await _userMonutoringService.Client.GetFullChat(chat);
            var users = await _userMonutoringService.Client.Users_GetUsers();
            var reaction = full.full_chat.AvailableReactions[0]; // choose the first available reaction emoji



            for (int offset_id = 700000; ;)
            {
                var messages = await _userMonutoringService.Client.Messages_GetHistory(chat, offset_id, offset_date: DateTime.Now, limit: 10);
                if (messages.Messages.Length == 0) break;
                foreach (var msgBase in messages.Messages)
                    if (msgBase is Message msg)
                        Console.WriteLine(msg.message);
                offset_id = messages.Messages[^1].ID;
            }
            return Ok();
        }


        [HttpPost("sendMessage")]
        public async Task<object> SendMessageChat(long id, string message)
        {
            if (_userMonutoringService.User == null) throw new Exception("Complete the login first");
            var target = _chats.chats[id];
            await _userMonutoringService.Client.SendMessageAsync(target, message);

            return Ok();
        }
    }
}
