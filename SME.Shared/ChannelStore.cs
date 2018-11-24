using System;
using System.Collections.Generic;

namespace SME.Shared
{

    public class ChannelStore
    {

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
            throw new ArgumentException($"Index {index} does not exist in channel store.");
        }

        /// <summary>
        /// Get all values in the store
        /// </summary>
        /// <returns></returns>
        public List<string> GetValues()
        {
            return _channelValues;
        }

    }
}
