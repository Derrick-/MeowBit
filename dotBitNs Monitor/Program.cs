using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dotBitNs_Monitor
{
    internal static class Program
    {
        public static EventHandler OnAdditionalInstanceSignal;

        [STAThread]
        static void Main(string[] args)
        {
            StartOrSignal(Run);
        }

        public static bool _Closing = false;
        private static AutoResetEvent m_Signal = new AutoResetEvent(true);
        public static void Set() { m_Signal.Set(); }
        private static void Run()
        {
            TimeSpan _oneMS = TimeSpan.FromMilliseconds(1);
            var window = new MainWindow();
            window.Show();
            window.Closed += window_Closed;
            System.Windows.Threading.Dispatcher.Run();
        }
       
        static void window_Closed(object sender, EventArgs e)
        {
            System.Windows.Threading.Dispatcher.ExitAllFrames();
        }

        static bool useGlobalMutex = false;
        private static void StartOrSignal(Action Run)
        {
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format(useGlobalMutex ? "Global\\{{{0}}}" : "{{{0}}}", appGuid);
            string SingleAppComEventName = mutexId + "_event";

            BackgroundWorker singleAppComThread = null;
            EventWaitHandle threadComEvent = null;

            using (var mutex = new Mutex(false, mutexId))
            {
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);

                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(1000, false);
                        if (hasHandle == false)
                        {
                            Debug.WriteLine("Instance already running, timeout expired");

                            threadComEvent = EventWaitHandle.OpenExisting(SingleAppComEventName);
                            threadComEvent.Set();  // signal the other instance.
                            threadComEvent.Close();

                            return;
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                        hasHandle = true;
                    }
                    threadComEvent = new EventWaitHandle(false, EventResetMode.AutoReset, SingleAppComEventName);
                 
                    singleAppComThread = new BackgroundWorker();
                    singleAppComThread.WorkerReportsProgress = false;
                    singleAppComThread.WorkerSupportsCancellation = true;
                    singleAppComThread.DoWork += (object sender, DoWorkEventArgs e) =>
                        {
                            BackgroundWorker worker = sender as BackgroundWorker;
                            WaitHandle[] waitHandles = new WaitHandle[] { threadComEvent };

                            while (!worker.CancellationPending && !_Closing)
                            {
                                // check every second for a signal.
                                if (WaitHandle.WaitAny(waitHandles, 1000) == 0)
                                {
                                    // The user tried to start another instance. We can't allow that, 
                                    // so bring the other instance back into view and enable that one. 
                                    // That form is created in another thread, so we need some thread sync magic.
                                    if (OnAdditionalInstanceSignal != null)
                                        OnAdditionalInstanceSignal(sender, new EventArgs());
                                }
                            }
                        };
                    singleAppComThread.RunWorkerAsync();

                    Run();

                    singleAppComThread.CancelAsync();
                    while (singleAppComThread.IsBusy)
                        Thread.Sleep(50);
                    threadComEvent.Close();

                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }
    }
}
