using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
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
            var parentRel = workItem.Relations.FirstOrDefault(t => t.Rel == "System.LinkTypes.Hierarchy-Reverse");
            /*if (parentRel != null)
            {
                azureWit.Parent = await GetWorkItem(connInfo, witClient, parentRel.GetItemId());
            }*/
            var histories = await witClient.GetRevisionsAsync(id);
            var history = histories.Where(h => h.Fields.ContainsKey("System.AssignedTo")).OrderByDescending(h => h.Fields["System.ChangedDate"]).FirstOrDefault();
            azureWit.AssignedOn = history != null ? (DateTime)history.Fields["System.ChangedDate"] : DateTime.MinValue;
            azureWit.Children = (await Task.WhenAll(tasks)).OrderBy(x => x.BacklogPriority).ToList();
            return azureWit;
        }

        public async static Task<IQueryable<AzureWorkItem>> GetWorkItems(AzureDevOpsConnectionInfo connInfo, WorkItemTrackingHttpClient witClient, string whereCond)
        {
            var q = new Wiql()
            {
                Query = $"SELECT [System.Id] FROM WorkItems WHERE {whereCond}"
            };
            var result = await witClient.QueryByWiqlAsync(q);
            var wait = await Task.WhenAll(result.WorkItems.Select(x => GetWorkItem(connInfo, witClient, x.Id)));
            return wait.AsQueryable();
        }
    }
}
