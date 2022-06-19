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
        /// Список локаций
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VenueCommand>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<VenueCommand>>> GetAllVenuesAsync()
        {
            IEnumerable<VenueCommand> list = await _commandService.VenueCommandServise.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Добавление локации
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VenueCommand))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VenueCommand>> AddNewVenueAsync(VenueRequest botCallerRequest)
        {
            VenueCommand command = _mapper.Map<VenueCommand>(botCallerRequest);
            await _commandService.VenueCommandServise.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление локации бота
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VenueCommand))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateVenueAsync(VenueRequestUpdate botCallerRequest)
        {
            VenueCommand command = _mapper.Map<VenueCommand>(botCallerRequest);
            await _commandService.VenueCommandServise.UpdateAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление локаций бота
        /// </summary>
        /// <param name="botCallerRequestsList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VenueCommand))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateVenuesListAsync(IEnumerable<VenueRequestUpdate> botCallerRequestsList)
        {
            IEnumerable<VenueCommand> commandsList = _mapper.Map<IEnumerable<VenueCommand>>(botCallerRequestsList);
            await _commandService.VenueCommandServise.UpdateListAsync(commandsList);
            return Ok();
        }
        /// <summary>
        /// Удаление локации
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VenueCommand))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteVenueByIdAsync(int id)
        {
            VenueCommand entity = await _commandService.VenueCommandServise.GetByIdAsync(id);
            await _commandService.VenueCommandServise.DeleteAsync(entity);
            return Ok();
        }
    }
}
