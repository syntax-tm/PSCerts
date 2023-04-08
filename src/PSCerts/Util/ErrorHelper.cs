using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace PSCerts.Util
{
    public static class ErrorHelper
    {
        public class ExceptionInfo
        {
            private readonly Exception _exception;

            public ErrorCategory ErrorCategory { get; private set; }
            public ErrorCode ErrorCode { get; private set; }
            public string ErrorID { get; private set; }

            public ExceptionInfo(Exception ex)
            {
                _exception = ex;

                Categorize();
            }
            
            private void Categorize()
            {
                ErrorCode = _exception switch
                {
                    ArgumentNullException _       => ErrorCode.ArgumentMissing,
                    DirectoryNotFoundException _  => ErrorCode.NotFound,
                    FileNotFoundException _       => ErrorCode.NotFound,
                    KeyNotFoundException          => ErrorCode.NotFound,
                    CryptographicException _      => ErrorCode.Permissions,
                    SecurityException _           => ErrorCode.Permissions,
                    UnauthorizedAccessException _ => ErrorCode.Permissions,
                    JsonException _               => ErrorCode.InvalidFormat,
                    _                             => ErrorCode.None
                };

                ErrorCategory = ErrorCode switch
                {
                    ErrorCode.InvalidFormat   => ErrorCategory.InvalidData,
                    ErrorCode.NotFound        => ErrorCategory.ObjectNotFound,
                    ErrorCode.Permissions     => ErrorCategory.PermissionDenied,
                    ErrorCode.ArgumentFormat  => ErrorCategory.InvalidArgument,
                    ErrorCode.ArgumentEmpty   => ErrorCategory.InvalidArgument,
                    ErrorCode.ArgumentMissing => ErrorCategory.InvalidArgument,
                    _                         => ErrorCategory.InvalidOperation
                };

                var exceptionType = _exception.GetType().Name;

                ErrorID = ErrorCode == ErrorCode.None
                    ? $"{ErrorCode.GetDescription()}.{exceptionType}"
                    : ErrorCode.GetDescription();
            }
        }

        public static ErrorRecord CreateError(Exception exception)
        {
            var info = new ExceptionInfo(exception);
            
            return new (exception, info.ErrorID, info.ErrorCategory, null);
        }
    }
}
