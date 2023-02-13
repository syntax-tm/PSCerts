using System;
using System.Collections.Generic;
using System.Linq;

namespace PSCerts
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Any();
        public List<string> Errors { get; }

        public ValidationResult()
        {
            Errors = new ();
        }
        
        public ValidationResult(string error)
        {
            Errors = new () { error };
        }

        public ValidationResult(IEnumerable<string> errors)
        {
            Errors = new (errors);
        }
        
        public ValidationResult(IEnumerable<ValidationResult> errors)
        {
            Errors = new ();
            Add(errors);
        }

        public void Add(string error)
        {
            Errors.Add(error);
        }

        public void Add(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }
        
        public void Add(ValidationResult error)
        {
            Errors.AddRange(error.Errors);
        }

        public void Add(IEnumerable<ValidationResult> errors)
        {
            Errors.AddRange(errors.SelectMany(e => e.Errors));
        }

        public void AssertIsValid()
        {
            if (IsValid) return;

            // ReSharper disable once UnthrowableException
            throw AsException();
        }

        public Exception AsException()
        {
            if (IsValid) return null;

            //var message = string.Format(@"Validation was not successful.{0}{0}Details:{0}{1}", Environment.NewLine, this);
            return new InvalidOperationException(ToString());
        }

        public override string ToString()
        {
            if (IsValid) return string.Empty;
            return string.Join(Environment.NewLine, Errors);
        }
    }
}
