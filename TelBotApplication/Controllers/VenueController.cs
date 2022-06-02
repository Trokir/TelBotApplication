using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public VenueController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список команд бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<VenueCommand>>> GetAllCommandsAsync()
        {
            IEnumerable<VenueCommand> list = await _commandService.VenueCommandServise.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление команды для бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ActionResult<VenueCommand>> AddnewCommandAsync(VenueRequest botCallerRequest)
        {
            VenueCommand command = _mapper.Map<VenueCommand>(botCallerRequest);
            await _commandService.VenueCommandServise.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ActionResult> UpdateCommandAsync(VenueRequestUpdate botCallerRequest)
        {
            VenueCommand command = _mapper.Map<VenueCommand>(botCallerRequest);
            await _commandService.VenueCommandServise.UpdateAsync(command);
            return Ok();
        }

        [HttpPut("updatelist")]
        public async Task<ActionResult> UpdateCommandsListAsync(IEnumerable<VenueRequestUpdate> botCallerRequestsList)
        {
            IEnumerable<VenueCommand> commandsList = _mapper.Map<IEnumerable<VenueCommand>>(botCallerRequestsList);
            await _commandService.VenueCommandServise.UpdateListAsync(commandsList);
            return Ok();
        }

        [HttpDelete("deletebyid")]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            VenueCommand entity = await _commandService.VenueCommandServise.GetByIdAsync(id);
            await _commandService.VenueCommandServise.DeleteAsync(entity);
            return Ok();
        }
    }
}
