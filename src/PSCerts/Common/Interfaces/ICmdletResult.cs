using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSCerts
{
    public interface ICmdletResult<T>
        where T : class
    {
        List<T> Output { get; }
        List<ErrorRecord> Errors { get; }
        List<string> Warnings { get; }
        
        bool HasOutput { get; }
        bool HasErrors { get; }
        bool HasWarnings { get; }
    }
}
