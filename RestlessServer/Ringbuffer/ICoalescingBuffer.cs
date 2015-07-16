using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulands.Restless.Ringbuffer
{
    public interface ICoalescingBuffer<K, V> {

        int Size { get; }
        int Capacity { get; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        bool Offer(K key, V value);
        bool Offer(V value);
        int Poll(ICollection<V> bucket);
        int Poll(ICollection<V> bucket, int maxItems);

    }
}
