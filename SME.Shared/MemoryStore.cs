using System.Collections.Generic;

namespace SME.Shared
{

    public class MemoryStore
    {
        private Dictionary<int, ChannelStore> _stores = new Dictionary<int, ChannelStore>();

        public void Store(int id, string value)
        {
            if (!_stores.ContainsKey(id))
            {
                _stores.Add(id, new ChannelStore());
            }
            _stores[id].Store(value);
        }

        public string Read(int id, int index)
        {
            return _stores[id].Read(index);
        }
    }
}
