using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS
{
    public delegate void CrashedEventHandler(CrashedEventArgs e);
    public delegate void ShutdownEventHandler(ShutdownEventArgs e);

    public static class EventSink
    {
        public static event CrashedEventHandler Crashed;
        public static event ShutdownEventHandler Shutdown;

        public static void InvokeShutdown(ShutdownEventArgs e)
        {
            if (Shutdown != null)
                Shutdown(e);
        }

        public static void InvokeCrashed(CrashedEventArgs e)
        {
            if (Crashed != null)
                Crashed(e);
        }

    }

    public class ShutdownEventArgs : EventArgs
    {
        public ShutdownEventArgs()
        {
        }
    }

    public class CrashedEventArgs : EventArgs
    {
        private Exception m_Exception;
        private bool m_Close;

        public Exception Exception { get { return m_Exception; } }
        public bool Close { get { return m_Close; } set { m_Close = value; } }

        public CrashedEventArgs(Exception e)
        {
            m_Exception = e;
        }
    }

}
