using AutoMapper;
using Microsoft.TeamFoundation.Core.WebApi;

namespace AzureDevOpsDataContextDriver
{
    public static class TeamProjectReferenceExtension
    {
        public static AzureProject Map(this TeamProjectReference item)
        {
            MapperConfiguration configuration = new MapperConfiguration(config =>
            {
                config.AddProfile<AzureProjectProfile>();
            });
            configuration.AssertConfigurationIsValid();
            IMapper mapper = configuration.CreateMapper();
            return mapper.Map<AzureProject>(item);
        }
    }
}
