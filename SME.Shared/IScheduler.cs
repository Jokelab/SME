using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public interface IScheduler
    {
        void Schedule(IEnumerable<CodeTransformation> codeTransformations, string[] args);
        Verdict GetVerdict(List<Channel> outputChannels);
    }
}
