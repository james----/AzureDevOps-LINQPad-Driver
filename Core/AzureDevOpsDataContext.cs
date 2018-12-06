using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataContextDriver.External
{
    public class AzureDevOpsDataContext : IDisposable
    {
        VssConnection vSSConnection;
        WorkItemTrackingHttpClient witClient;
        ProjectHttpClient projectHttpClient;
        AzureDevOpsConnectionInfo connectionInfo;

        public AzureDevOpsDataContext(AzureDevOpsConnectionInfo connInfo)
        {
            connectionInfo = connInfo ?? throw new ArgumentNullException("connInfo");
            vSSConnection = new VssConnection(new Uri(connInfo.Uri), new VssBasicCredential("pat", connInfo.Token));
            witClient = vSSConnection.GetClient<WorkItemTrackingHttpClient>();
            projectHttpClient = vSSConnection.GetClient<ProjectHttpClient>();
        }

        public async Task<AzureWorkItem> GetWorkItem(int id)
        {
            return await AzureDevOpsWorkItemFactory.GetWorkItem(connectionInfo, witClient, id);
        }


        #region IDisposable Support
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
