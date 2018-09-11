using SME.Scheduler.Php;
using SME.Shared;
using SME.Transformer.Php;

namespace SME.Factory
{
    public class PhpFactory : SmeFactory
    {
        public override IScheduler CreateScheduler()
        {
            return new PhpScheduler();
        }

        public override ITransformer CreateTransformer()
        {
            return new PhpTransformer();
        }
    }
}
