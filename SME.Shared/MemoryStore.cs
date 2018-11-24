using System.Collections.Generic;

namespace SME.Shared
{

    public class MemoryStore
    {
        /// <summary>
        /// Dictionary with the channel ID as key, and a channelstore object as value.
        /// </summary>
        private Dictionary<int, ChannelStore> _stores = new Dictionary<int, ChannelStore>();
        private Dictionary<string, int> _readCount = new Dictionary<string, int>();

        /// <summary>
        /// Store a value for a channel by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Store(int id, string value)
        {
            if (!_stores.ContainsKey(id))
            {
                _stores.Add(id, new ChannelStore());
            }
            _stores[id].Store(value);
        }

        /// <summary>
        /// Read a stored value from the memory store.
        /// The key is composed of the id and level.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public string Read(int id, int level)
        {
            var key = $"{id}_{level}"; 
            if (!_readCount.ContainsKey(key))
            {
                _readCount[key] = 0;
            }
            else
            {
                _readCount[key]++;
            }
            return _stores[id].Read(_readCount[key]);
        }

        /// <summary>
        /// Retrieve a channel store without incrementing the read count
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ChannelStore GetChannel(int id)
        {
            if (_stores.ContainsKey(id))
            {
                return _stores[id];
            }
            return null;
        }
    }
}
