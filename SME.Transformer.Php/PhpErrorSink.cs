using Devsense.PHP.Errors;
using Devsense.PHP.Text;
using System.Collections.Generic;

namespace SME.Transformer.Php
{
    public class PhpErrorSink : IErrorSink<Span>
    {
        public class ErrorInstance
        {
            public Span Span;
            public ErrorInfo Error;
            public string[] Args;

            public override string ToString() => Error.ToString(Args);
        }

        public readonly List<ErrorInstance> Errors = new List<ErrorInstance>();

        public int Count => this.Errors.Count;

        public void Error(Span span, ErrorInfo info, params string[] argsOpt)
        {
            Errors.Add(new ErrorInstance()
            {
                Span = span,
                Error = info,
                Args = argsOpt,
            });
        }
    }
}
