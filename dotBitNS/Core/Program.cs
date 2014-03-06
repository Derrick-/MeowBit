// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace dotBitNS
{
    public delegate void Slice();

    internal static class Program
    {
        private enum ConsoleEventType
        {
            CTRL_C_EVENT,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        public static Slice Slice;

        private static bool m_Crashed;
        private static Thread timerThread;
        private static Assembly m_Assembly;
        private static Process m_Process;
        private static Thread m_Thread;
        private static string m_ExePath;
        private static string m_WorkingFolder;
        static string m_Name = null;

        private static MultiTextWriter m_MultiConOut;
        private static FileLogger m_Logger;
        public static bool LoggingEnabled
        {
            get { return (m_Logger != null) ? m_Logger.Enabled : false; }
            set { if (m_Logger != null) m_Logger.Enabled = value; }
        }

        private static long m_CycleIndex = 1;
        private static float[] m_CyclesPerSecond = new float[100];

        private static AutoResetEvent m_Signal = new AutoResetEvent(true);
        public static void Set() { m_Signal.Set(); }

        private static bool m_Closing;
        public static bool Closing { get { return m_Closing; } }

        public static string Arguments { get; set; }
        public static bool IsService { get; private set; }
        public static bool TestMode { get; private set; }
        public static bool LoggingRequested { get; private set; }

        public static bool Debug { get; private set; }
        public static Assembly Assembly { get { return m_Assembly; } set { m_Assembly = value; } }
        public static Version Version { get { return m_Assembly.GetName().Version; } }
        public static string Name { get { return m_Name ?? (m_Name = m_Assembly.FullName.Split(',')[0]); } }
        public static Process Process { get { return m_Process; } }
        public static Thread Thread { get { return m_Thread; } }

        public static string ExePath
        {
            get
            {
                if (m_ExePath == null)
                {
                    //m_ExePath = Assembly.Location;
                    m_ExePath = Process.GetCurrentProcess().MainModule.FileName;
                }

                return m_ExePath;
            }
        }

        public static string WorkingFolder
        {
            get
            {
                if (m_WorkingFolder == null)
                    m_WorkingFolder = new FileInfo(ExePath).Directory.FullName;

                return m_WorkingFolder;
            }
        }

        public static float CyclesPerSecond
        {
            get { return m_CyclesPerSecond[(m_CycleIndex - 1) % m_CyclesPerSecond.Length]; }
        }

        public static float AverageCPS
        {
            get
            {
                float t = 0.0f;
                int c = 0;

                for (int i = 0; i < m_CycleIndex && i < m_CyclesPerSecond.Length; ++i)
                {
                    t += m_CyclesPerSecond[i];
                    ++c;
                }

                return (t / Math.Max(c, 1));
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static WaitHandle[] waitHandles = new WaitHandle[] { new AutoResetEvent(false) };

        static void Main(string[] args)
        {
            Arguments = string.Join(" ", args);

            if (args.Length == 1 && IsServiceCommand(args[0]))
            {
                if (Insensitive.Equals(args[0], "-restart"))
                {
                    Service.ServiceControlRestart();
                }
                else if (Insensitive.Equals(args[0], "-stop"))
                {
                    Service.ServiceControlStop();
                }
                else if (Insensitive.Equals(args[0], "-start"))
                {
                    Service.ServiceControlStart();
                }
                else if (Insensitive.Equals(args[0], "-install"))
                {
                    ServiceInstaller si = new ServiceInstaller();
                    if (si.InstallService(Assembly.GetExecutingAssembly().Location + " -service", Service.GlobalServiceName, Service.GlobalServiceName + " Service"))
                        Console.WriteLine("The " + Service.GlobalServiceName + " service has been installed.");
                    else
                        Console.WriteLine("An error occurred during service installation.");
                }
                else if (Insensitive.Equals(args[0], "-uninstall"))
                {
                    ServiceInstaller si = new ServiceInstaller();
                    if (si.UnInstallService(Service.GlobalServiceName))
                        Console.WriteLine("The " + Service.GlobalServiceName + " service has been uninstalled. You may need to reboot to remove it completely.");
                    else
                        Console.WriteLine("An error occurred during service removal.");
                }
                else if (Insensitive.Equals(args[0], "-setauto"))
                {
                    ServiceInstaller si = new ServiceInstaller();
                    if (si.SetAutoAtart(Service.GlobalServiceName))
                        Console.WriteLine("The " + Service.GlobalServiceName + " service has been set to auto-start.");
                    else
                        Console.WriteLine("An error occurred during service modification.");
                }
                return;
            }

            foreach (var arg in args)
            {
                if (Insensitive.Equals(arg, "-debug"))
                    Debug = true;
                else if (Insensitive.Equals(arg, "-test"))
                    TestMode = true;
                else if (Insensitive.Equals(arg, "-service"))
                    IsService = true;
                else if (Insensitive.Equals(arg, "-log"))
                    LoggingRequested = true;
                else if (IsServiceCommand(arg))
                {
                    Console.WriteLine("Service commands may not be used with other arguments");
                    return;
                }
            }

            StartIfNotRunning();
        }

        private static void StartIfNotRunning()
        {
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

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
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            {
                                Console.WriteLine("Instance already running, timeout expired");
                                return;
                            }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                        hasHandle = true;
                    }

                    if (IsService)
                    {
                        ServiceBase[] ServicesToRun;
                        ServicesToRun = new ServiceBase[] 
			            { 
				            new Service()
			            };
                        ServiceBase.Run(ServicesToRun);
                    }
                    else
                    {
                        Run();
                    }
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        private static bool IsServiceCommand(string arg)
        {
            return
                Insensitive.Equals(arg, "-install") ||
                Insensitive.Equals(arg, "-uninstall") ||
                Insensitive.Equals(arg, "-setauto") ||
                Insensitive.Equals(arg, "-restart") ||
                Insensitive.Equals(arg, "-stop") ||
                Insensitive.Equals(arg, "-start");
        }

        internal static void Run()
        {

            LoadSettings();
            InitializeLogging();

            Console.WriteLine("Starting...");

            m_Thread = Thread.CurrentThread;
            m_Process = Process.GetCurrentProcess();
            m_Assembly = Assembly.GetEntryAssembly();

            if (m_Thread != null)
                m_Thread.Name = "Core Thread";

            Timer.TimerThread ttObj = new Timer.TimerThread();
            timerThread = new Thread(new ThreadStart(ttObj.TimerMain));
            timerThread.Name = "Timer Thread";

            Version ver = m_Assembly.GetName().Version;

            Configure();

            Initialize();

            m_ConsoleEventHandler = new ConsoleEventHandler(OnConsoleEvent);
            SetConsoleCtrlHandler(m_ConsoleEventHandler, true);

            timerThread.Start();

            try
            {
                DateTime now, last = DateTime.UtcNow;

                const int sampleInterval = 100;
                const float ticksPerSecond = (float)(TimeSpan.TicksPerSecond * sampleInterval);
                TimeSpan _oneMS = TimeSpan.FromMilliseconds(1);

                long sample = 0;

                while (!m_Closing)
                {
                    m_Signal.WaitOne(_oneMS);
                    Timer.Slice();
                    if (Slice != null)
                        Slice();
                    if ((++sample % sampleInterval) == 0)
                    {
                        now = DateTime.UtcNow;
                        m_CyclesPerSecond[m_CycleIndex++ % m_CyclesPerSecond.Length] =
                            ticksPerSecond / (now.Ticks - last.Ticks);
                        last = now;
                    }
                }
            }
            catch (Exception e)
            {
                CurrentDomain_UnhandledException(null, new UnhandledExceptionEventArgs(e, true));
            }
            finally
            {
                UI.Monitor.DisableCacheEntries();
            }
        }

        private static void InitializeLogging()
        {
            string datecode = DateTime.Now.ToString("yyyyMMddHHmmss");
            string file = string.Format("{0}.log", datecode);
            string path = Path.Combine(Program.WorkingFolder, @"logs\console\");
            Directory.CreateDirectory(path);
            m_Logger = new FileLogger(file, path, enabled: LoggingRequested);
            Console.SetOut(m_MultiConOut = new MultiTextWriter(Console.Out, m_Logger));
        }

        private static void Configure()
        {
            Invoke(new Assembly[] { Assembly }, "Configure");
        }

        private static void LoadSettings()
        {
            LoggingRequested |= IsService & ConfigurationManager.AppSettings.Get("ServiceLogging") == "true";
        }

        private static void Initialize()
        {
            Invoke(new Assembly[] { Assembly }, "Initialize");
        }

        public static void Invoke(Assembly[] m_Assemblies, string method)
        {
            List<MethodInfo> invoke = new List<MethodInfo>();

            for (int a = 0; a < m_Assemblies.Length; ++a)
            {
                Type[] types = m_Assemblies[a].GetTypes();

                for (int i = 0; i < types.Length; ++i)
                {
                    MethodInfo m = types[i].GetMethod(method, BindingFlags.Static | BindingFlags.Public);

                    if (m != null)
                        invoke.Add(m);
                }
            }

            invoke.Sort(new CallPriorityComparer());

            for (int i = 0; i < invoke.Count; ++i)
                invoke[i].Invoke(null, null);
        }

        static void GetKey(Object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            Console.ReadKey(false);
            are.Set();
        }

        public static void Kill()
        {
            Kill(false);
        }

        public static void Kill(bool restart)
        {
            HandleClosed();

            if (restart)
                Process.Start(ExePath, Arguments);

            if(!IsService)
                m_Process.Kill();
        }

        private static void HandleClosed()
        {
            if (m_Closing)
                return;

            m_Closing = true;

            Console.Write("Exiting...");

            if (!m_Crashed)
                EventSink.InvokeShutdown(new ShutdownEventArgs());

            Timer.TimerThread.Set();

            Console.WriteLine("done");
        }

        private delegate bool ConsoleEventHandler(ConsoleEventType type);
        private static ConsoleEventHandler m_ConsoleEventHandler;

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventHandler callback, bool add);

        private static bool OnConsoleEvent(ConsoleEventType type)
        {
            if ((IsService && type == ConsoleEventType.CTRL_LOGOFF_EVENT))
                return true;

            Kill();

            return true;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            HandleClosed();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.IsTerminating ? "Error:" : "Warning:");
            Console.WriteLine(e.ExceptionObject);

            if (e.IsTerminating)
            {
                m_Crashed = true;

                bool close = false;

                try
                {
                    CrashedEventArgs args = new CrashedEventArgs(e.ExceptionObject as Exception);

                    EventSink.InvokeCrashed(args);

                    close = args.Close;
                }
                catch
                {
                }

                if (!close && !IsService)
                {
                    Console.WriteLine("This exception is fatal, press return to exit");
                    Console.ReadLine();
                }

                Kill();
            }
        }

        internal static void Continue()
        {
            Console.WriteLine("Pause and Continue not implemented...");
        }

        internal static void Pause()
        {
            Console.WriteLine("Pause and Continue not implemented...");
        }
    }
}
