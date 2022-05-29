using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotCommandController : ControllerBase
    {
        private readonly IBotCommandService _botCommandService;
        private readonly IMapper _mapper;
        public BotCommandController(IBotCommandService botCommandService, IMapper mapper)
        {
            _botCommandService = botCommandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список команд бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<BotCaller>>> GetAllCommandsAsync()
        {
            var list = await _botCommandService.GetAllCommandsAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление команды для бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ActionResult<BotCaller>> AddnewCommandAsync(BotCallerRequest botCallerRequest)
        {
            var command = _mapper.Map<BotCaller>(botCallerRequest);
            await _botCommandService.AddNewCommandAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ActionResult<BotCaller>> UpdateCommandAsync(BotCallerRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<BotCaller>(botCallerRequest);
            var result = await _botCommandService.UpdateEntityAsync(command);
            return new OkObjectResult(result);
        }

        [HttpPut("updatelist")]
        public async Task<ActionResult<IEnumerable<BotCaller>>> UpdateCommandsListAsync(IEnumerable<BotCallerRequestForUpdate> botCallerRequestsList)
        {
            var commandsList = _mapper.Map<IEnumerable<BotCaller>>(botCallerRequestsList);
            var result = await _botCommandService.UpdateEntitiesListAsync(commandsList);
            return new OkObjectResult(result);
        }
        [HttpDelete("deletebycommand")]
        public async Task<ActionResult> DeleteCommandByCommandAsync(string command)
        {
            await _botCommandService.DeleteCommandByCommandAsync(command);
            return Ok();
        }
        [HttpDelete("deletebyid")]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            await _botCommandService.DeleteCommandByIdAsync(id);
            return Ok();
        }

    }
}
