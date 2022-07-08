using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BotCaller>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BotCaller>>> GetAllCommandsAsync()
        {
            var list = await _commandService.BotCommandService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление команды для бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BotCaller))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BotCaller>> AddnewCommandAsync(BotCallerRequest botCallerRequest)
        {
            var command = _mapper.Map<BotCaller>(botCallerRequest);
            await _commandService.BotCommandService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCommandAsync(BotCallerRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<BotCaller>(botCallerRequest);
            await _commandService.BotCommandService.UpdateAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление списка команд
        /// </summary>
        /// <param name="botCallerRequestsList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCommandsListAsync(IEnumerable<BotCallerRequestForUpdate> botCallerRequestsList)
        {
            var commandsList = _mapper.Map<IEnumerable<BotCaller>>(botCallerRequestsList);
            await _commandService.BotCommandService.UpdateListAsync(commandsList);
            return Ok();
        }
        /// <summary>
        /// Удаление команды бота
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            var entity = await _commandService.BotCommandService.GetByIdAsync(id);
            await _commandService.BotCommandService.DeleteAsync(entity);
            return Ok();
        }

    }
}
