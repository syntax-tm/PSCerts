using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSCerts.Commands;
using PSCerts.Util;

namespace PSCerts
{
    public static class CmdletBaseExtensions
    {
        public static ICmdletResult<T> GetResult<T>(this CmdletBase cmdlet)
            where T : class
        {
            var result = new MockCommandRuntime<T>();
            cmdlet.CommandRuntime = result;
            cmdlet.Execute();

            return result;
        }
        
        public static List<ErrorRecord> GetErrors(this CmdletBase cmdlet)
        {
            var result = new MockCommandRuntime<object>();
            cmdlet.CommandRuntime = result;
            cmdlet.Execute();
            return result.Errors;
        }
        
        public static List<T> GetResults<T>(this CmdletBase cmdlet)
            where T : class
        {
            return GetResults<T>(cmdlet, new (), null);
        }
        
        public static List<T> GetResults<T>(this CmdletBase cmdlet, List<ErrorRecord> errors, List<string> warnings)
            where T : class
        {
            var output = new List<T>();
            cmdlet.CommandRuntime = new MockCommandRuntime<T>(output, errors, warnings);
            cmdlet.Execute();
            return output;
        }

        public static void ThrowException(this Cmdlet cmdlet, Exception e)
        {
            var error = ErrorHelper.CreateError(e);
            cmdlet.WriteError(error);
        }

        public static void ThrowTerminatingException(this Cmdlet cmdlet, Exception e)
        {
            var error = ErrorHelper.CreateError(e);
            cmdlet.ThrowTerminatingError(error);
        }
    }
}
