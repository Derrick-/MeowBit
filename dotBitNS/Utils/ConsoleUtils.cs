using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS
{
    public class ConsoleUtils
    {
        public abstract class ColorObject : IDisposable
        {
            public abstract ConsoleColor Color { get; }

            public ColorObject()
            {
                PushColor(Color);
            }

            public void Dispose()
            {
                PopColor();
            }
        }

        public class Warning : ColorObject
        {
            public override ConsoleColor Color
            {
                get { return ConsoleColor.Blue; }
            }
        }

        public class Info : ColorObject
        {
            public override ConsoleColor Color
            {
                get { return ConsoleColor.White; }
            }
        }

        public static void WriteWarning(string format, params object[] args)
        {
            WriteWarning(string.Format(format, args));
        }
        public static void WriteWarning(string text)
        {
            using (var warning = new ConsoleUtils.Warning())
                Console.WriteLine(text);
        }

        public static void WriteInfo(string format, params object[] args)
        {
            WriteInfo(string.Format(format, args));
        }
        public static void WriteInfo(string text)
        {
            using (var info = new ConsoleUtils.Info())
                Console.WriteLine(text);
        }


        private static Stack<ConsoleColor> m_ConsoleColors = new Stack<ConsoleColor>();
        public static void PushColor(ConsoleColor color)
        {
            try
            {
                m_ConsoleColors.Push(Console.ForegroundColor);
                Console.ForegroundColor = color;
            }
            catch
            {
            }
        }

        public static void PopColor()
        {
            try
            {
                Console.ForegroundColor = m_ConsoleColors.Pop();
            }
            catch
            {
            }
        }

    }
}
