// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS
{
    public static class ReflectionUtils
    {
        public delegate void RegisterObjectHandler(Type t, object[] attrs);

        public static void RegisterObjects<TBaseClass, TAttribute>(RegisterObjectHandler registerMethod)
        {
            List<Type> types = FindInheritedTypes(typeof(TBaseClass));
            types.Sort(new TypePriorityComparer());
            foreach (var t in types)
            {
                object[] attrs = t.GetCustomAttributes(typeof(TAttribute), false);
                if (attrs.Length > 0)
                {
                    registerMethod(t, attrs);
                }

            }
        }

        public static List<Type> FindInheritedTypes(Type parenttype)
        {
            List<Type> list = new List<Type>();
            foreach (var t in Program.Assembly.GetTypes())
            {
                if (t != parenttype && parenttype.IsAssignableFrom(t))
                    list.Add(t);
            }
            return list;
        }
    }
}
