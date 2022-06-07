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
    public class GroupController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public GroupController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список команд бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<Group>>> GetAllCommandsAsync()
        {
            IEnumerable<Group> list = await _commandService.GroupService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление команды для бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ActionResult<Admin>> AddnewCommandAsync(GroupDTO botCallerRequest)
        {
            var command = _mapper.Map<Group>(botCallerRequest);
            await _commandService.GroupService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ActionResult> UpdateCommandAsync(GroupRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<Group>(botCallerRequest);
            await _commandService.GroupService.UpdateAsync(command);
            return Ok();
        }

        [HttpPut("updatelist")]
        public async Task<ActionResult> UpdateCommandsListAsync(IEnumerable<GroupRequestForUpdate> botCallerRequestsList)
        {
            var commandsList = _mapper.Map<IEnumerable<Group>>(botCallerRequestsList);
            await _commandService.GroupService.UpdateListAsync(commandsList);
            return Ok();
        }

        [HttpDelete("deletebyid")]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            var entity = await _commandService.GroupService.GetByIdAsync(id);
            await _commandService.GroupService.DeleteAsync(entity);
            return Ok();
        }
    }
}
