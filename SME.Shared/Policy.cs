using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class Policy : IPolicy
    {
        public List<ChannelLabel> Input { get; } = new List<ChannelLabel>();
        public List<ChannelLabel> Output { get; } = new List<ChannelLabel>();

        public List<ChannelLabel> Sanitize { get; } = new List<ChannelLabel>();

        public List<SecurityLevel> Levels { get; } = new List<SecurityLevel>();

    }
}
