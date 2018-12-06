using LINQPad.Extensibility.DataContext;
using System.Xml.Linq;

namespace AzureDevOpsDataContextDriver
{
    public class AzureDevOpsConnectionInfo
    {
        readonly IConnectionInfo _cxInfo;
        readonly XElement _driverData;

        public AzureDevOpsConnectionInfo(IConnectionInfo cxInfo)
        {
            _cxInfo = cxInfo;
            _driverData = cxInfo.DriverData;
        }

        public bool Persist
        {
            get { return _cxInfo.Persist; }
            set { _cxInfo.Persist = value; }
        }

        public string Uri
        {
            get { return (string)_driverData.Element("Uri") ?? string.Empty; }
            set { _driverData.SetElementValue("Uri", value); }
        }

        public string Token
        {
            get { return (string)_driverData.Element("Domain") ?? string.Empty; }
            set { _driverData.SetElementValue("Domain", value); }
        }
    }
}
