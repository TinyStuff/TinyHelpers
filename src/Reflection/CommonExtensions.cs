using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    public interface IRunOnCopy
    {
        void DataCopied(object fromObject);
    }

    public static class GenericExtensions
    {
        public static IEnumerable<(PropertyInfo prp, T attr)> GetPropertiesWithAttribute<T>(this object o) where T : Attribute
        {
            var attrType = typeof(T);
            Type typ = null;
            if (o is Type t)
            {
                typ = t;
            }
            else
            {
                typ = o.GetType();
            }
            foreach (var prp in typ.GetRuntimeProperties())
            {
                var customAttr = prp.GetCustomAttributes(attrType, true).OfType<T>().FirstOrDefault();
                if (customAttr!=null) {
                    yield return (prp, customAttr);
                }
            }
        }

        public static IEnumerable<(MethodInfo prp, T attr)> GetMethodsWithAttribute<T>(this object o) where T : Attribute
        {
            var attrType = typeof(T);
            Type typ = null;
            if (o is Type t)
            {
                typ = t;
            }
            else
            {
                typ = o.GetType();
            }
            foreach (var prp in typ.GetRuntimeMethods())
            {
                var customAttr = prp.GetCustomAttributes(attrType, true).OfType<T>().FirstOrDefault();
                if (customAttr != null)
                {
                    yield return (prp, customAttr);
                }
            }
        }

        public static void MemberviseCopyTo(this object a, object b, bool overwrite = true)
        {
            var aType = a.GetType();
            var bType = b.GetType();
            var copyAttr = typeof(CopyAttribute);
            foreach (var prpInfo in bType.GetRuntimeProperties())
            {
                if (prpInfo.CanWrite)
                {
                    try
                    {
                        var customAttr = prpInfo.GetCustomAttributes(copyAttr, true).OfType<CopyAttribute>().FirstOrDefault();
                        var copyFromName = prpInfo.Name;
                        if (customAttr == null || !customAttr.Exclude)
                        {
                            if (customAttr != null && !string.IsNullOrEmpty(customAttr.FromProperty))
                            {
                                copyFromName = customAttr.FromProperty;
                            }
                            var fromProperty = aType.GetRuntimeProperty(copyFromName);
                            if (fromProperty != null && fromProperty.CanRead)
                            {
                                var valueToAdd = fromProperty.GetValue(a, null);
                                if (valueToAdd != null)
                                {
                                    if (overwrite || !IsEmpty(valueToAdd))
                                        prpInfo.SetValue(b, valueToAdd);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var i = 22;
                    }

                }
            }
            var runOnCopy = b as IRunOnCopy;
            if (runOnCopy != null)
            {
                runOnCopy.DataCopied(a);
            }
        }

        private static bool IsEmpty(object valueToAdd)
        {
            if (valueToAdd == null)
                return true;
            if (valueToAdd is string s)
            {
                return string.IsNullOrEmpty(s);
            }
            if (valueToAdd is int i)
            {
                return i != 0;
            }
            if (valueToAdd is double d)
            {
                return d != 0;
            }
            if (valueToAdd is float f)
            {
                return f != 0;
            }
            if (valueToAdd is DateTime date)
            {
                return date > DateTime.MinValue;
            }
            return false;
        }
    }
}
