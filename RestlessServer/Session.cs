using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Nulands.Restless
{
    public class Session
    {
        public ConcurrentDictionary<Type, object> Data { get; set; }
        public DateTime CreationDate { get; private set; }
        public DateTime ValidUntil { get; set; }

        public Session()
        {
            Data = new ConcurrentDictionary<Type, object>();
            CreationDate = DateTime.UtcNow;
        }

        public T Get<T>()
        {
            T result = default(T);
            object tmp = null;
            if (Data.TryGetValue(typeof(T), out tmp))
                result = (T)tmp;
            return result;
        }

        public void Set<T>(T value)
        {
            Data[typeof(T)] = value;
        }
    }
}
