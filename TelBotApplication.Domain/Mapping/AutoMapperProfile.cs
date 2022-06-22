using AutoMapper;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;
using TelBotApplication.Domain.Models.Anchors;

namespace TelBotApplication.Domain.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            _ = CreateMap<VenueRequest, VenueCommand>();
            _ = CreateMap<VenueCommand, VenueRequest>();
            _ = CreateMap<VenueRequestUpdate, VenueCommand>();
            _ = CreateMap<BotCallerRequest, BotCaller>();
            _ = CreateMap<BotCallerRequestForUpdate, BotCaller>();
            _ = CreateMap<BotCommandDto, BotCaller>();
            _ = CreateMap<BotCaller, BotCommandDto>();
            _ = CreateMap<TextFilterDTO, TextFilter>();
            _ = CreateMap<TextFilter, TextFilterDTO>();
            _ = CreateMap<Anchor, AnchorDTO>()
                .ForMember(x => x.ButtonCondition, opt => opt.MapFrom(z => z.AnchorCallback.ButtonCondition))
                .ForMember(x => x.ButtonText, opt => opt.MapFrom(z => z.AnchorCallback.ButtonText));

            _ = CreateMap<AnchorCallbackDTO, AnchorCallback>();
            _ = CreateMap<AnchorCallback, AnchorCallbackDTO>();
        }
    }
}
