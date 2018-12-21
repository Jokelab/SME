using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.Shared
{
    public class Verdict
    {

        public TransformationResult TransformationResult { get; set; }

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

        /// <summary>
        /// Show test case verdict
        /// </summary>
        /// <param name="verdict"></param>
        public string Show(IPolicy policy, string poFileContent)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("===== VERDICT =====");
            if (InterferenceDetected())
            {
                //code is interferent
                builder.AppendLine($"Observed {InterferentChannels.Count} channel(s) with different output values between the SME exection and the original execution.\nThe code is thus interferent.");
                foreach (var chan in InterferentChannels)
                {
                    var levelName = policy.Levels.Where(l => l.Level == chan.Label.Level).Select(l => l.Name).FirstOrDefault();
                    builder.AppendLine($"\n=> Channel ID {chan.Id} (level {levelName})\n");
                    var code = chan.Location.GetText(poFileContent);
                    builder.AppendLine($"Code: {code}");
                    builder.AppendLine($"\nPosition in original code:  {chan.Location.GetLocation(poFileContent)}\n");
                    builder.AppendLine($"Captured differences:\n{Messages[chan.Id]}");
                }

            }
            else
            {
                //code shows non-interference for the provided input values
                builder.AppendLine("No interferent channels detected for the provided input values.");
            }

            return builder.ToString();
        }

    }
}