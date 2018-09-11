
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
   public interface ITransformer
    {
        TransformationResult Transform(string code, IPolicy policy);
    }
}
