using System;
using System.IO;
using JsonSubTypes;
using Newtonsoft.Json;
using PSCerts.Util;

namespace PSCerts.Config
{
    [JsonConverter(typeof(JsonSubtypes), nameof(SourceType))]
    [JsonSubtypes.KnownSubType(typeof(FileSource), SOURCE_TYPE_FILE)]
    [JsonSubtypes.KnownSubType(typeof(TextSource), SOURCE_TYPE_TEXT)]
    [JsonSubtypes.KnownSubType(typeof(EnvironmentVariableSource), SOURCE_TYPE_ENV)]
    public abstract class CertPassword : IValidate
    {
        protected const string SOURCE_TYPE_FILE = @"file";
        protected const string SOURCE_TYPE_ENV = @"env";
        protected const string SOURCE_TYPE_TEXT = @"text";

        [JsonProperty("value", Required = Required.Always)]
        protected string Value { get; set; }

        public abstract string SourceType { get; }
        public abstract string GetValue();
        public virtual ValidationResult Validate() => new (@"Invalid Type");
    }

    public class FileSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_FILE;

        public string ResolvedPath
        {
            get
            {
                var resolvedPath = FileSystemHelper.ResolvePath(Value);
                return resolvedPath.FullName;
            }
        }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) return new ($@"{nameof(Value)} is required.");
            if (!File.Exists(ResolvedPath)) return new ($@"Password file '{Value}' does not exist.");

            return new ();
        }

        public override string GetValue()
        {
            var validationResult = Validate();
            validationResult.AssertIsValid();

            var resolvedPath = FileSystemHelper.ResolvePath(Value);
            var content = File.ReadAllText(resolvedPath.FullName);
            
            return content;
        }
    }

    public class TextSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_TEXT;

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) return new ($@"{nameof(Value)} is required.");

            return new ();
        }

        public override string GetValue()
        {
            var validationResult = Validate();
            validationResult.AssertIsValid();

            return Value;
        }
    }

    public class EnvironmentVariableSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_ENV;

        public EnvironmentVariableTarget? Target { get; set; }
        
        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) return new ($@"{nameof(Value)} is required.");
            
            var variable = Environment.GetEnvironmentVariable(Value);
            
            if (string.IsNullOrWhiteSpace(variable)) return new ($@"Environment variable '{Value}' is empty.");

            return new ();
        }

        public override string GetValue()
        {
            var validationResult = Validate();
            validationResult.AssertIsValid();

            var variable = Target.HasValue
                ? Environment.GetEnvironmentVariable(Value, Target.Value)
                : Environment.GetEnvironmentVariable(Value);

            return variable;
        }
    }
}
