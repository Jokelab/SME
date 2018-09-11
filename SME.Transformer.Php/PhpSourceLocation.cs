using Devsense.PHP.Text;
using SME.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Transformer.Php
{
    public class PhpSourceLocation: ISourceLocation
    {
        public PhpSourceLocation(Span location)
        {
            Location = location;
        }
        public Span Location { get; set; }
    }
}
