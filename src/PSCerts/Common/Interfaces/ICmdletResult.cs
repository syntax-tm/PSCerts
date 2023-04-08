using System.Collections.Generic;
using System.Management.Automation;

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
