using System;
using SME.Shared;


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
            throw new NotImplementedException();
        }
    }
}
