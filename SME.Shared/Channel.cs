using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class Channel
    {
        /// <summary>
        /// The unique channel identifier for the channel instance
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Reference to the channel definition in the active policy.
        /// </summary>
        public ChannelLabel Label { get; set; }

        /// <summary>
        /// Reference to the location in the original source code.
        /// </summary>
        public ISourceLocation Location { get; set; }

    }
}
