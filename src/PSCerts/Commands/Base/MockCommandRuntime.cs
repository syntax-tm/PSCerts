using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PSCerts.Commands
{
    public class MockCommandRuntime<T> : ICommandRuntime, ICmdletResult<T>
        where T : class
    {
        private readonly List<T> _output;
        private readonly List<ErrorRecord> _errors;
        private readonly List<string> _warnings;

        public List<T> Output => _output;
        public List<ErrorRecord> Errors => _errors;
        public List<string> Warnings => _warnings;

        public bool HasOutput => Output.Any();
        public bool HasErrors => Errors.Any();
        public bool HasWarnings => Warnings.Any();
        
        public MockCommandRuntime()
        {
            _output = new ();
            _errors = new ();
            _warnings = new ();
        }
        
        public MockCommandRuntime(List<T> output, List<ErrorRecord> errors = null, List<string> warnings = null)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _errors = errors;
            _warnings = warnings;
        }
        
        public bool ShouldContinue(string query, string caption) => true;

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) => true;

        public bool ShouldProcess(string target) => true;

        public bool ShouldProcess(string target, string action) => true;

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) => true;

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            _errors.Add(errorRecord);
        }

        public bool TransactionAvailable()
        {
            return false;
        }

        public void WriteCommandDetail(string text)
        {
        }

        public void WriteDebug(string text)
        {
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            _errors?.Add(errorRecord);
        }

        public void WriteObject(object sendToPipeline)
        {
            _output.Add(sendToPipeline as T);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                _output.Add(sendToPipeline as T);
            }
            else
            {
                var enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        _output.Add(enumerator.Current as T);
                    }
                }
                else
                {
                    _output.Add(sendToPipeline as T);
                }
            }
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
        }

        public void WriteVerbose(string text)
        {
        }

        public void WriteWarning(string text)
        {
            _warnings?.Add(text);
        }
        
        // ReSharper disable once ThrowExceptionInUnexpectedLocation
        public PSTransactionContext CurrentPSTransaction => throw new NotImplementedException();
        
        // ReSharper disable once ThrowExceptionInUnexpectedLocation
        public PSHost Host => throw new NotImplementedException();
    }
}
