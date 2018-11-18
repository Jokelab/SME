using System;
using System.Collections.Generic;

namespace SME.Shared
{

    public class ChannelStore
    {
        public int ChannelId { get; set; }

        private List<string> _channelValues = new List<string>();

        public void Store(string value)
        {
            _channelValues.Add(value);
        }

        public string Read(int index)
        {
            if (index < _channelValues.Count)
            {
                return _channelValues[index];
            }
            throw new ArgumentException($"Index {index} does not exist in store for channel {ChannelId}");
        }

    }
}
