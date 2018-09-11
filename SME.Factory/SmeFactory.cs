using SME.Shared;

namespace SME.Factory
{
    public abstract class SmeFactory
    {
        public abstract IScheduler CreateScheduler();
        public abstract ITransformer CreateTransformer();
    }
}
