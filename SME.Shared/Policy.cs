using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class Policy : IPolicy
    {
        public List<ChannelLabel> InputLabels { get; } = new List<ChannelLabel>();
        public List<ChannelLabel> OutputLabels { get; } = new List<ChannelLabel>();

        public List<SecurityLevel> Levels { get; } = new List<SecurityLevel>();

    }
}
