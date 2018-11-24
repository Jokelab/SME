using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.Shared
{
    public class OutputComparer
    {

        public static Verdict Compare(List<Channel> outputChannels, MemoryStore smeStore, MemoryStore originalStore)
        {
            var outcome = new Verdict();

            //determine lowest output level
            var minLevel = outputChannels.Min(chan => chan.Label.Level);

            //all channels higher than the minimum could be interferent
            var highOutputChannels = outputChannels.Where(chan => chan.Label.Level >= minLevel).ToList();

            //check each channel for differences between the original execution and the sme execution
            foreach (var channel in highOutputChannels)
            {
                //retrieve the corresponding channel stores from memory
                var originalChannel = originalStore.GetChannel(channel.Id) ?? new ChannelStore();
                var smeChannel = smeStore.GetChannel(channel.Id) ?? new ChannelStore();


                var smeValues = smeChannel.GetValues();
                var originalValues = originalChannel.GetValues();

                //Now check if the observed values are equal.
                for (var i = 0; i < Math.Max(smeValues.Count, originalValues.Count); i++)
                {
                    string smeVal = "[no output]";
                    string originalVal = "[no output]";
                    if (i < smeValues.Count)
                    {
                        smeVal = smeValues[i];
                    }
                    if (i < originalValues.Count)
                    {
                        originalVal = originalValues[i];
                    }

                    if (!smeVal.Equals(originalVal))
                    {
                        //found a difference, so report it as being interferent
                        if (!outcome.Messages.ContainsKey(channel.Id))
                        {
                            outcome.Messages[channel.Id] = new StringBuilder();
                            outcome.InterferentChannels.Add(channel);
                        }
                        outcome.Messages[channel.Id].AppendLine($"#{i + 1} Original\t: {originalVal}");
                        outcome.Messages[channel.Id].AppendLine($"#{i + 1} SME\t\t: {smeVal}");
                    }
                }
            }
            return outcome;


        }
    }
}
