using System.Collections.Generic;

namespace SME.Shared
{
    public interface IPolicy
    {
        List<SecurityLevel> Levels { get; }
        List<ChannelLabel> InputLabels { get;  }
        List<ChannelLabel> OutputLabels { get;  }
        
    }
}