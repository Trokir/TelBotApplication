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
        /// Список адимнистраторов
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(IEnumerable<Admin>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAllAdminsAsync()
        {
            IEnumerable<Admin> list = await _commandService.AdminService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление нового админа
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Admin))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddNewAdminAsync(AdminDTO botCallerRequest)
        {
            var command = _mapper.Map<Admin>(botCallerRequest);
           await _commandService.AdminService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление адимнистратора
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAdminAsync(AdminRequestForUpdate botCallerRequest)
        {
            var command = _mapper.Map<Admin>(botCallerRequest);
            await _commandService.AdminService.UpdateAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление списка админов
        /// </summary>
        /// <param name="botCallerRequestsList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAdminsListAsync(IEnumerable<AdminRequestForUpdate> botCallerRequestsList)
        {
            IEnumerable<Admin> commandsList = _mapper.Map<IEnumerable<Admin>>(botCallerRequestsList);
            await _commandService.AdminService.UpdateListAsync(commandsList);
            return Ok();
        }
        /// <summary>
        /// Удаление админа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAdminByIdAsync(int id)
        {
            var entity = await _commandService.AdminService.GetByIdAsync(id);
            await _commandService.AdminService.DeleteAsync(entity);
            return Ok();
        }
    }
}
