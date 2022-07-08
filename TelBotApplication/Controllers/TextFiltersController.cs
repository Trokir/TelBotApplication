using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextFiltersController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public TextFiltersController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }

        /// <summary>
        /// Список фильтров
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TextFilter>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TextFilter>>> GetAllFiltersAsync()
        {
            var list = await _commandService.TextFilterService.GetAllAsync();
            return Ok(list);
        }
        /// <summary>
        /// Список фильтров
        /// </summary>
        /// <returns></returns>
        [HttpGet("getallbyType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TextFilter>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TextFilter>>> GetAllFiltersAsync(TypeOfFilter ofFilter)
        {
            var list = await _commandService.TextFilterService.GetAllAsync(x => x.Filter == ofFilter);
            return Ok(list);
        }



        /// <summary>
        /// Добавление нового фильтра (ключевого слова)
        /// </summary>
        /// <param name="filterRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TextFilter))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddNewFilterAsync(TextFilterDTO filterRequest)
        {
            var filter = _mapper.Map<TextFilter>(filterRequest);
            await _commandService.TextFilterService.AddAsync(filter);
            return Ok();
        }

        /// <summary>
        /// Список фильтров
        /// </summary>
        /// <returns></returns>
        [HttpGet("getbyword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TextFilter))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TextFilter>> GetFilterByWordAsync(string word)
        {
            var list = await _commandService.TextFilterService.FindAsync(x => x.Text.Equals(word, System.StringComparison.InvariantCultureIgnoreCase));
            return Ok(list);
        }



        /// <summary>
        /// Обновление фильтров
        /// </summary>
        /// <param name="filterRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateFilterAsync(TextFilterDTO filterRequest)
        {
            var filter = _mapper.Map<TextFilter>(filterRequest);
            await _commandService.TextFilterService.UpdateAsync(filter);
            return Ok();
        }

        /// <summary>
        /// Обновление списка фильтров
        /// </summary>
        /// <param name="filterList"></param>
        /// <returns></returns>
        [HttpPut("updatelist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateFiltersListAsync(IEnumerable<TextFilterDTO> filterList)
        {
            var filters = _mapper.Map<IEnumerable<TextFilter>>(filterList);
            await _commandService.TextFilterService.UpdateListAsync(filters);
            return Ok();
        }

        /// <summary>
        /// Удаление фильтра
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deletebyid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteFilterByIdAsync(int id)
        {
            var entity = await _commandService.TextFilterService.GetByIdAsync(id);
            await _commandService.TextFilterService.DeleteAsync(entity);
            return Ok();
        }

    }
}
