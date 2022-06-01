

using AutoMapper;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Domain.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            _ = CreateMap<BotCallerRequest, BotCaller>();
            _ = CreateMap<BotCallerRequestForUpdate, BotCaller>();
            _ = CreateMap<BotCommandDto, BotCaller>();
            _ = CreateMap<BotCaller, BotCommandDto>();
            _ = CreateMap<MessageModelDTO, MessageModel>();
            _ = CreateMap<MessageModel, MessageModelDTO>();
            _ = CreateMap<MessageModelDTOWithId, MessageModel>();
            _ = CreateMap<MessageModel, MessageModelDTOWithId>();
        }
    }
}
