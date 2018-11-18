using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class ChannelLabel
    {
        public string Name { get; set; }
        public int Level { get; set; }

        /// <summary>
        /// If it is an input channel, then this refers to the level after running the sanitize method.
        /// </summary>
        public int? TargetLevel { get; set; }

    }
}
