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
        /// Список чатов бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Group>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Group>>> GetAllGroupsAsync()
        {
            IEnumerable<Group> list = await _commandService.GroupService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление чата
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Group))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Group>> AddNewGroupAsync(GroupDTO botCallerRequest)
        {
            var command = _mapper.Map<Group>(botCallerRequest);
            await _commandService.GroupService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление чата
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Group))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateGroupAsync(GroupRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<Group>(botCallerRequest);
            await _commandService.GroupService.UpdateAsync(command);
            return Ok();
        }
        /// <summary>
        /// Удаление чата
        /// </summary>
        /// <param name="botCallerRequestsList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Group))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateGroupsListAsync(IEnumerable<GroupRequestForUpdate> botCallerRequestsList)
        {
            var commandsList = _mapper.Map<IEnumerable<Group>>(botCallerRequestsList);
            await _commandService.GroupService.UpdateListAsync(commandsList);
            return Ok();
        }
        /// <summary>
        /// Удаление чата
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Group))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteGroupByIdAsync(int id)
        {
            var entity = await _commandService.GroupService.GetByIdAsync(id);
            await _commandService.GroupService.DeleteAsync(entity);
            return Ok();
        }
    }
}
