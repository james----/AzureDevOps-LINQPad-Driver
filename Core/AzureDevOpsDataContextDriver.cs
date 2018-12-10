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
            return new AzureDevOpsConnectionInfo(cxInfo).Url;
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
            return new[]
            {
                "System.Data.Linq",
                "System.Data.Linq.SqlClient",
                "AzureDevOpsDataContextDriver",
                "System.Data",
                "AsyncFixer",
                "AutoMapper"
            };
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
            if (context is AzureDevOpsDataContext rc)
            {
                rc.Dispose();
            }
        }
    }
}
