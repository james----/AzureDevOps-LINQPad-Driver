using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataContextDriver.External
{
    public class AzureDevOpsDataContext : IDisposable
    {
        VssConnection vSSConnection;
        ProjectHttpClient projectHttpClient;
        WorkItemTrackingHttpClient witClient;
        AzureDevOpsConnectionInfo connectionInfo;
        AzureWorkItemQueryProvider dataProvider;

        public AzureDevOpsDataContext(AzureDevOpsConnectionInfo connInfo)
        {
            connectionInfo = connInfo ?? throw new ArgumentException($"{nameof(connInfo)} can't be null");
            vSSConnection = new VssConnection(new Uri(connectionInfo.Url), new VssBasicCredential("pat", connInfo.Token));
            projectHttpClient = vSSConnection.GetClient<ProjectHttpClient>();
            witClient = vSSConnection.GetClient<WorkItemTrackingHttpClient>();
            dataProvider = new AzureWorkItemQueryProvider(connInfo, witClient);
        }

        public async Task<AzureWorkItem> GetWorkItem(int id)
        {
            return await AzureDevOpsWorkItemFactory.GetWorkItem(connectionInfo, witClient, id);
        }

        public IEnumerable<AzureProject> Projects => AzureDevOpsProjectFactory.GetProjects(connectionInfo, projectHttpClient).GetAwaiter().GetResult();

        public IQueryable<AzureWorkItem> WorkItems => dataProvider;

        #region IDisposable
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    projectHttpClient.Dispose();
                    projectHttpClient.Dispose();
                    vSSConnection.Dispose();
                }

                projectHttpClient = null;
                projectHttpClient = null;
                vSSConnection = null;

                disposedValue = true;
            }
        }

        ~AzureDevOpsDataContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
