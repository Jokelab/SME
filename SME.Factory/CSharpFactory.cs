using System;
using SME.Shared;
using SME.Transformer.CSharp;

namespace SME.Factory
{
    public class CSharpFactory : SmeFactory
    {
        public override IScheduler CreateScheduler()
        {
            throw new NotImplementedException();
        }

        public override ITransformer CreateTransformer()
        {
            return new CSharpTransformer();
        }
    }
}
