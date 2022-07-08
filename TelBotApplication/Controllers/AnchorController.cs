using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models.Anchors;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnchorController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public AnchorController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список якорей
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Anchor>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AnchorForUpdate>>> GetAllAnchorsAsync()
        {
            var list = await _commandService.AnchorService.GetAllAsync();
            var result = _mapper.Map<IEnumerable<AnchorForUpdate>>(list);
            return Ok(result);
        }
        /// <summary>
        /// Добавление якоря
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Anchor))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddNewAnchorAsync(AnchorDTO botCallerRequest)
        {
            var command = _mapper.Map<Anchor>(botCallerRequest);
            await _commandService.AnchorService.AddAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление якоря
        /// </summary>
        /// <param name="botCallerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAnchorAsync(AnchorForUpdate botCallerRequest)
        {
            var command = _mapper.Map<Anchor>(botCallerRequest);
            await _commandService.AnchorService.UpdateAsync(command);
            return Ok();
        }
        /// <summary>
        /// Обновление якоря
        /// </summary>
        /// <param name="botCallerRequestsList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAdminsListAsync(IEnumerable<AnchorForUpdate> botCallerRequestsList)
        {
            IEnumerable<Anchor> commandsList = _mapper.Map<IEnumerable<Anchor>>(botCallerRequestsList);
            await _commandService.AnchorService.UpdateListAsync(commandsList);
            return Ok();
        }
        /// <summary>
        /// Удаление якоря
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAdminByIdAsync(int id)
        {
            var entity = await _commandService.AnchorService.GetByIdAsync(id);
            await _commandService.AnchorService.DeleteAsync(entity);
            return Ok();
        }
        /// <summary>
        /// Удаление всех якорей
        /// </summary>
        /// <returns></returns>
        [HttpGet("deleteall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Anchor>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Anchor>>> DeleteAllAnchorsAsync()
        {
            await _commandService.AnchorService.DeleteRangeAsync(x=>!string.IsNullOrEmpty(x.Tag));
            return Ok();
        }

        
    }
}
