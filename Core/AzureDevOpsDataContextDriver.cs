using AzureDevOpsDataContextDriver.External;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AzureDevOpsDataContextDriver.AzureDevOps
{
    public class AzureDevOpsDataContextDriver : StaticDataContextDriver
    {
        public override string Name => "Azure DevOps Driver";

        public override string Author => "Soham Dasgupta <soham1.dasgupta@gmail.com>";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return new AzureDevOpsConnectionInfo(cxInfo).Uri;
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            return new List<ExplorerItem>();
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var win = new ConnectionDialog(cxInfo);
            var result = win.ShowDialog() == true;
            if (result)
            {
                cxInfo.CustomTypeInfo.CustomAssemblyPath = Assembly.GetAssembly(typeof(AzureDevOpsDataContext)).Location;
                cxInfo.CustomTypeInfo.CustomTypeName = "AzureDevOpsDataContextDriver.External.AzureDevOpsDataContext";
            }
            return result;
        }

        public override IEnumerable<string> GetNamespacesToRemove(IConnectionInfo cxInfo)
        {
            return new[] { "System.Data.Linq", "System.Data.Linq.SqlClient" };
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            var namespaces = new List<string>(base.GetNamespacesToAdd(cxInfo));
            namespaces.AddRange(new[] { "AzureDevOpsDataContextDriver.External" });
            return namespaces;
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[] { new ParameterDescriptor("connInfo", "AzureDevOpsDataContextDriver.AzureDevOpsConnectionInfo") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            return new[] { new AzureDevOpsConnectionInfo(cxInfo) };
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            base.TearDownContext(cxInfo, context, executionManager, constructorArguments);
            var rc = context as AzureDevOpsDataContext;
            if (rc != null)
            {
                rc.Dispose();
            }
        }

        /*public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            return base.GetCustomDisplayMemberProvider(objectToWrite);
        }

        public override void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            base.PreprocessObjectToWrite(ref objectToWrite, info);
        }

        public override void DisplayObjectInGrid(object objectToDisplay, GridOptions options)
        {
            base.DisplayObjectInGrid(objectToDisplay, options);
        }

        public override DbProviderFactory GetProviderFactory(IConnectionInfo cxInfo)
        {
            return base.GetProviderFactory(cxInfo);
        }

        public override IDbConnection GetIDbConnection(IConnectionInfo cxInfo)
        {
            return base.GetIDbConnection(cxInfo);
        }

        public override void ExecuteESqlQuery(IConnectionInfo cxInfo, string query)
        {
            base.ExecuteESqlQuery(cxInfo, query);
        }

        public override string GetAppConfigPath(IConnectionInfo cxInfo)
        {
            return base.GetAppConfigPath(cxInfo);
        }

        public override object OnCustomEvent(string eventName, params object[] data)
        {
            return base.OnCustomEvent(eventName, data);
        }

        public override object InitializeLifetimeService()
        {
            return base.InitializeLifetimeService();
        }

        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            return base.AreRepositoriesEquivalent(c1, c2);
        }*/
    }
}
