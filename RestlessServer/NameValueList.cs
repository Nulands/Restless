using System;

namespace Nulands.Restless
{
    public class NameValueList
    {
        public Func<string, string> RetreiveFunc { get; set; }
        public Action<string, string> AddFunc { get; set; }

        public string this[string name]
        {
            get
            {
                if (RetreiveFunc != null)
                    return RetreiveFunc(name);
                return String.Empty;
            }
            set
            {
                if (AddFunc != null)
                    AddFunc(name, value);
            }
        }
    }
}
