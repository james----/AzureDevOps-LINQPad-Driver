using AutoMapper;
using Microsoft.TeamFoundation.Core.WebApi;

namespace AzureDevOpsDataContextDriver
{
    public class AzureProjectProfile : Profile
    {
        public AzureProjectProfile()
        {
            CreateMap<TeamProjectReference, AzureProject>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}
