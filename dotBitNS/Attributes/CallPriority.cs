using System;
using System.Collections.Generic;
using System.Reflection;

namespace dotBitNS
{

    public enum MemberPriority : byte
    {
        Lowest=0,
        BelowNormal=50,
        Normal=100,
        AboveNormal=150,
        Highest=255,
    }

    public class CallPriorityComparer : BasePriorityComparer<MethodInfo, CallPriorityAttribute> { }
    public class TypePriorityComparer : BasePriorityComparer<Type, TypePriorityAttribute> { }

    [AttributeUsage(AttributeTargets.Method)]
    public class CallPriorityAttribute : BasePriorityAttribute
    {
        public CallPriorityAttribute(MemberPriority priority) : base(priority) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TypePriorityAttribute : BasePriorityAttribute
    {
        public TypePriorityAttribute(MemberPriority priority) : base(priority) { }
    }

    public abstract class BasePriorityAttribute : Attribute
    {
        private MemberPriority m_Priority;

        public MemberPriority Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }

        public BasePriorityAttribute(MemberPriority priority)
        {
            m_Priority = priority;
        }
    }

    public abstract class BasePriorityComparer<Tmember, Tattr> : IComparer<Tmember>
        where Tmember : System.Runtime.InteropServices._MemberInfo
        where Tattr : BasePriorityAttribute
    {
        protected object[] GetAttributes(Tmember t)
        {
            object[] objs = t.GetCustomAttributes(typeof(Tattr), true);
            return objs;
        }

        public int Compare(Tmember x, Tmember y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return 1;

            if (y == null)
                return -1;

            return GetPriority(x) - GetPriority(y);
        }

        private MemberPriority GetPriority(Tmember mi)
        {
            object[] objs = GetAttributes(mi);

            if (objs == null)
                return 0;

            if (objs.Length == 0)
                return 0;

            Tattr attr = objs[0] as Tattr;

            if (attr == null)
                return 0;

            return attr.Priority;
        }
    }
}
