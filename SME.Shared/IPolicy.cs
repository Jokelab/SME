using System.Collections.Generic;

namespace SME.Shared
{
    public interface IPolicy
    {
        List<SecurityLevel> Levels { get; }
        List<ChannelLabel> Input { get; }
        List<ChannelLabel> Output { get; }
        List<ChannelLabel> Sanitize { get; }

    }
}