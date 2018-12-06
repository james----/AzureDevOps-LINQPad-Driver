using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataContextDriver
{
    public static class AzureDevOpsWorkItemFactory
    {
        public async static Task<AzureWorkItem> GetWorkItem(AzureDevOpsConnectionInfo connInfo, WorkItemTrackingHttpClient witClient, int id)
        {
            var workItem = await witClient.GetWorkItemAsync(id, expand: WorkItemExpand.All);
            var azureWit = workItem.Map(connInfo);
            var tasks = workItem.Relations.Where(t => t.Rel == "System.LinkTypes.Hierarchy-Forward").Select(x => GetWorkItem(connInfo, witClient, x.GetItemId()));
            var histories = await witClient.GetRevisionsAsync(id);
            var history = histories.Where(h => h.Fields.ContainsKey("System.AssignedTo")).OrderByDescending(h => h.Fields["System.ChangedDate"]).FirstOrDefault();
            azureWit.AssignedOn = history != null ? (DateTime)history.Fields["System.ChangedDate"] : DateTime.MinValue;
            azureWit.Children = (await Task.WhenAll(tasks)).OrderBy(x => x.BacklogPriority).ToList();
            return azureWit;
        }
    }
}
