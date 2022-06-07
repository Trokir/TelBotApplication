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
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public AdminController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список команд бота
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAllCommandsAsync()
        {
            IEnumerable<Admin> list = await _commandService.AdminService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление команды для бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ActionResult<Admin>> AddnewCommandAsync(AdminDTO botCallerRequest)
        {
            var command = _mapper.Map<Admin>(botCallerRequest);
            await _commandService.AdminService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление команды бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ActionResult> UpdateCommandAsync(AdminRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<Admin>(botCallerRequest);
            await _commandService.AdminService.UpdateAsync(command);
            return Ok();
        }

        [HttpPut("updatelist")]
        public async Task<ActionResult> UpdateCommandsListAsync(IEnumerable<AdminRequestForUpdate> botCallerRequestsList)
        {
            IEnumerable<Admin> commandsList = _mapper.Map<IEnumerable<Admin>>(botCallerRequestsList);
            await _commandService.AdminService.UpdateListAsync(commandsList);
            return Ok();
        }

        [HttpDelete("deletebyid")]
        public async Task<ActionResult> DeleteCommandByIdAsync(int id)
        {
            var entity = await _commandService.AdminService.GetByIdAsync(id);
            await _commandService.AdminService.DeleteAsync(entity);
            return Ok();
        }
    }
}
