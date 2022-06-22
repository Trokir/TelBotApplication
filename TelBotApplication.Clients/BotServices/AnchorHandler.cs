using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models.Anchors;

namespace TelBotApplication.Clients.BotServices
{
    public class AnchorHandler : IAnchorHandler
    {
        private readonly ILogger<AnchorHandler> _logger;
        private readonly IServiceProvider _factory;
        private HashSet<AnchorDTO> _anchors = default;
        private readonly IMapper _mapper;
        public AnchorHandler(IServiceProvider factory, ILogger<AnchorHandler> logger, IMapper mapper)
        {
            _factory = factory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task UpdateAchors()
        {
            _logger.LogDebug("Start UpdateAchors");
            using IServiceScope scope = _factory.CreateScope();
            while (true)
            {

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IUnitOfWork>();

                var result = (await context.AnchorService.GetAllAsync()).ToHashSet();
                _anchors = _mapper.Map<HashSet<AnchorDTO>>(result);

                await Task.Delay(20000);
            }
        }

        public async Task ExecuteAncor(string text)
        {
            if (_anchors is HashSet<AnchorDTO> keys)
            {
                if (keys.Any(x=>x.Tag.Equals(text.Trim(),StringComparison.InvariantCultureIgnoreCase)))
                {
                    var anchor = keys.SingleOrDefault(x => x.Tag.Equals(text.Trim(), StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }
    }
}
