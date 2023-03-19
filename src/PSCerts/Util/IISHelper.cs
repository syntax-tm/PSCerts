using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSCerts.Util
{
    public static class IISHelper
    {
        private const string LOCAL_SERVICE = @"NT AUTHORITY\LOCAL SERVICE";
        private const string LOCAL_SYSTEM = @"NT AUTHORITY\SYSTEM";
        private const string NETWORK_SERVICE = @"NT AUTHORIT\NETWORK SERVICE";
        private const string IIS_APPPOOL_DOMAIN = @"IIS AppPool";

        public static string GetApplicationPoolIdentity(ApplicationPool appPool)
        {
            var processModel = appPool?.ProcessModel;
            if (processModel == null) throw new ArgumentNullException(nameof(processModel));

            return processModel.IdentityType switch
            {
                ProcessModelIdentityType.LocalService            => LOCAL_SERVICE,
                ProcessModelIdentityType.LocalSystem             => LOCAL_SYSTEM,
                ProcessModelIdentityType.NetworkService          => NETWORK_SERVICE,
                ProcessModelIdentityType.ApplicationPoolIdentity => $@"{IIS_APPPOOL_DOMAIN}\{appPool.Name}",
                _                                                => processModel.UserName
            };
        }
    }
}
