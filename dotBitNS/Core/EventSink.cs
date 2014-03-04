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
    public delegate void CrashedEventHandler(CrashedEventArgs e);
    public delegate void ShutdownEventHandler(ShutdownEventArgs e);

    internal delegate void NameServerAvailableChangedHandler(NmcClient source, NameServerAvailableChangedEventArgs e);

    public static class EventSink
    {
        public static event CrashedEventHandler Crashed;
        public static event ShutdownEventHandler Shutdown;

        internal static event NameServerAvailableChangedHandler NameServerAvailableChanged;


        internal static void InvokeShutdown(ShutdownEventArgs e)
        {
            if (Shutdown != null)
                Shutdown(e);
        }

        internal static void InvokeCrashed(CrashedEventArgs e)
        {
            if (Crashed != null)
                Crashed(e);
        }


        internal static void InvokeNameServerAvailableChanged(NmcClient source, NameServerAvailableChangedEventArgs e)
        {
            if (NameServerAvailableChanged != null)
                NameServerAvailableChanged(source, e);
        }

    }

    internal class NameServerAvailableChangedEventArgs : EventArgs
    {
        public bool Available {get;private set;}

        public NameServerAvailableChangedEventArgs(bool Available)
        {
            this.Available = Available;
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
