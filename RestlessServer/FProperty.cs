using System;

namespace Nulands.Restless
{
    public class FProperty : FProperty<object>
    {
        public static FProperty<T> Create<T>(Action<T> set)
        {
            return new FProperty<T>() { Set = set, Get = null };
        }
        public static FProperty<T> Create<T>(Func<T> get, Action<T> set = null)
        {
            return new FProperty<T>() { Set = set, Get = get };
        }
    }

    public class FProperty<T>
    {
        public bool IsReadable { get { return Get != null; } }
        public bool IsWriteable { get { return Set != null; } }

        public Func<T> Get { get; internal set; }
        public Action<T> Set { get; internal set; }
    }
}
