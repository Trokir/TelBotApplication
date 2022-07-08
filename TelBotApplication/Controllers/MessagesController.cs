using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IUnitOfWork _commandService;
        private readonly IMapper _mapper;
        public MessagesController(IUnitOfWork commandService, IMapper mapper)
        {
            _commandService = commandService;
            _mapper = mapper;
        }
        /// <summary>
        /// Список сообщений
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MessageLogger>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageLogger>>> GetAllMessagesAsync()
        {
            var list = await _commandService.MessageLoggerService.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Список сообщений
        /// </summary>
        /// <returns></returns>
        [HttpGet("getallbyDate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MessageLogger>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageLogger>>> GetAllMessagesAsync(DateTime dateTime)
        {

            var list = await _commandService.MessageLoggerService.GetAllAsync(x => x.AddedDate.Date == dateTime.Date);
            return Ok(list);
        }

        [HttpGet("getallbyuser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MessageLogger>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageLogger>>> GetAllMessagesAsync(DateTime dateTime, string userName)
        {
            var list = await _commandService.MessageLoggerService.GetAllAsync(x => x.AddedDate.Date == dateTime.Date && x.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
            return Ok(list);
        }

        [HttpDelete("deletebydate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MessageLogger>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageLogger>>> DeleteAllMessagesAsync(DateTime dateTime)
        {
            await _commandService.MessageLoggerService.DeleteRangeAsync(x => x.AddedDate.Date == dateTime.Date);
            return Ok();
        }
    }
}
