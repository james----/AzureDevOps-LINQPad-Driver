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

        public string Url
        {
            get { return (string)_driverData.Element("Url") ?? string.Empty; }
            set { _driverData.SetElementValue("Url", value); }
        }

        public string Token
        {
            get { return (string)_driverData.Element("Token") ?? string.Empty; }
            set { _driverData.SetElementValue("Token", value); }
        }
    }
}
