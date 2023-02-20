using System.Management.Automation;
using PSCerts.Util;

namespace PSCerts.Commands
{
    public abstract class CmdletBase : Cmdlet
    {
        public void Execute()
        {
            BeginProcessing();
            ProcessRecord();
            EndProcessing();
        }
    }
}
