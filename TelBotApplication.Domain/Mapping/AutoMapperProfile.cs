

using AutoMapper;
using System.Collections.Generic;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Domain.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<BotCallerRequest, BotCaller>();
            CreateMap<BotCallerRequestForUpdate, BotCaller>();
            CreateMap<BotCommandDto, BotCaller>();
            CreateMap<BotCaller, BotCommandDto>();

        }
    }
}
