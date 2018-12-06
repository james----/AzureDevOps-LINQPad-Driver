using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataContextDriver
{
    public static class AzureDevOpsProjectFactory
    {
        public async static Task<List<AzureProject>> GetProjects(AzureDevOpsConnectionInfo connInfo, ProjectHttpClient witClient, int id)
        {
            var items = await witClient.GetProjects();
            return items.Select(x => x.Map()).ToList();
        }
    }
}