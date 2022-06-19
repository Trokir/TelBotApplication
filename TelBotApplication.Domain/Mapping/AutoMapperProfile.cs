

using AutoMapper;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Domain.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            _ = CreateMap<Group, GroupRequestForUpdate>();
            _ = CreateMap<GroupRequestForUpdate, Group>();
            _ = CreateMap<GroupDTO, Group>();
            _ = CreateMap<Group, GroupDTO>();

            _ = CreateMap<Admin, AdminRequestForUpdate>();
            _ = CreateMap<AdminRequestForUpdate, Admin>();
            _ = CreateMap<AdminDTO, Admin>();
            _ = CreateMap<Admin, AdminDTO>();

            _ = CreateMap<VenueRequest, VenueCommand>();
            _ = CreateMap<VenueCommand, VenueRequest>();
            _ = CreateMap<VenueRequestUpdate, VenueCommand>();
            _ = CreateMap<BotCallerRequest, BotCaller>();
            _ = CreateMap<BotCallerRequestForUpdate, BotCaller>();
            _ = CreateMap<BotCommandDto, BotCaller>();
            _ = CreateMap<BotCaller, BotCommandDto>();
            _ = CreateMap<TextFilterDTO, TextFilter>();
            _ = CreateMap<TextFilter, TextFilterDTO>();

            
        }
    }
}
