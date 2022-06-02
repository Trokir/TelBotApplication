using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotCommandController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public BotCommandController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список команд бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<BotCaller>>> GetAllCommandsAsync()
        {
            IEnumerable<BotCaller> list = await _commandService.BotCommandService.GetAllAsync();
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
            BotCaller command = _mapper.Map<BotCaller>(botCallerRequest);
            await _commandService.BotCommandService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ActionResult> UpdateCommandAsync(BotCallerRequestForUpdate botCallerRequest)
        {
            BotCaller command = _mapper.Map<BotCaller>(botCallerRequest);
            await _commandService.BotCommandService.UpdateAsync(command);
            return Ok();
        }

        [HttpPut("updatelist")]
        public async Task<ActionResult> UpdateCommandsListAsync(IEnumerable<BotCallerRequestForUpdate> botCallerRequestsList)
        {
            IEnumerable<BotCaller> commandsList = _mapper.Map<IEnumerable<BotCaller>>(botCallerRequestsList);
            await _commandService.BotCommandService.UpdateListAsync(commandsList);
            return Ok();
        }

        [HttpDelete("deletebyid")]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            BotCaller entity = await _commandService.BotCommandService.GetByIdAsync(id);
            await _commandService.BotCommandService.DeleteAsync(entity);
            return Ok();
        }

    }
}
