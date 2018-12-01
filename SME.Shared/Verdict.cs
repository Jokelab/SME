using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public class Verdict
    {
        public List<Channel> InterferentChannels = new List<Channel>();
        public Dictionary<int, StringBuilder> Messages = new Dictionary<int, StringBuilder>();

        /// <summary>
        /// Indicate if there are any interferent channels found
        /// </summary>
        /// <returns></returns>
        public bool InterferenceDetected()
        {
            return InterferentChannels.Count > 0;
        }

    }
}