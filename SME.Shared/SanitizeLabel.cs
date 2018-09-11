using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class SanitizeLabel: ChannelLabel
    {
        /// <summary>
        /// The level after running the sanitize method
        /// </summary>
        public int SanitizedLevel { get; set; }
    }
}
