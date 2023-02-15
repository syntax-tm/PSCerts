using System.Management.Automation;
using PSCerts.Util;

namespace PSCerts.Commands
{
    public abstract class CmdletBase : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            PowerShellHelper.CurrentCmdlet = this;
        }
    }
}
