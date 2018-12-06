using AutoMapper;
using AzureDevOpsDataContextDriver;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models
{
    public static class WorkItemExtension
    {
        public static IEnumerable<int> GetChildItemIds(this WorkItem o)
        {
            foreach (var item in o.Relations.Where(t => t.Rel == "System.LinkTypes.Hierarchy-Forward"))
            {
                int id = 0;
                if (!string.IsNullOrWhiteSpace(o.Url))
                {
                    int.TryParse(o.Url.Split('/').Last(), out id);
                }
                yield return id;
            }
        }

        public static AzureWorkItem Map(this WorkItem item, AzureDevOpsConnectionInfo connectionInfo)
        {
            MapperConfiguration configuration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AzureWorkItemProfile(connectionInfo));
            });
            configuration.AssertConfigurationIsValid();
            IMapper mapper = configuration.CreateMapper();
            return mapper.Map<AzureWorkItem>(item);
        }
    }
}
