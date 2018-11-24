using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public interface ISourceLocation
    {
        string GetText(string code);
        string GetLocation();
    }
}
