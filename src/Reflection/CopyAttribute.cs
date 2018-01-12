using System;

namespace TinyHelpers.Reflection
{
    public class CopyAttribute : Attribute
    {
        public bool Exclude
        {
            get;
            set;
        }
        public string FromProperty { get; set; }
    }
}
