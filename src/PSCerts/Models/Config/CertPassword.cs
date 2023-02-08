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
    public abstract class CertPassword
    {
        protected const string SOURCE_TYPE_FILE = "file";
        protected const string SOURCE_TYPE_ENV = "env";
        protected const string SOURCE_TYPE_TEXT = "text";

        [JsonProperty("value", Required = Required.Always)]
        protected string Value { get; set; }

        public abstract string SourceType { get; }
        public virtual bool IsValid() => false;
        public abstract string GetValue();
    }

    public class FileSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_FILE;
        
        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Value)) return false;
            if (!File.Exists(Value)) return false;

            var resolvedPath = FileSystemHelper.ResolvePath(Value);

            return resolvedPath.Exists;
        }

        public override string GetValue()
        {
            if (!IsValid()) throw new InvalidOperationException($"Source is not valid.");

            var resolvedPath = FileSystemHelper.ResolvePath(Value);
            var content = File.ReadAllText(resolvedPath.FullName);
            
            return content;
        }
    }

    public class TextSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_TEXT;

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Value);
        public override string GetValue() => Value;
    }

    public class EnvironmentVariableSource : CertPassword
    {
        public override string SourceType => SOURCE_TYPE_ENV;

        public EnvironmentVariableTarget? Target { get; set; }
        
        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Value)) return false;

            var variable = Environment.GetEnvironmentVariable(Value);

            return !string.IsNullOrWhiteSpace(variable);
        }

        public override string GetValue()
        {
            if (!IsValid()) throw new InvalidOperationException($"Source is not valid.");

            var variable = Target.HasValue
                ? Environment.GetEnvironmentVariable(Value, Target.Value)
                : Environment.GetEnvironmentVariable(Value);

            return variable;
        }
    }
}
