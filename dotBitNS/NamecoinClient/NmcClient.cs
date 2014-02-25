using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotBitNS
{
    class NmcClient
    {
        static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5.0);

        public delegate void OnAvailableChangedHandler(NmcClient source, bool available);
        public static event OnAvailableChangedHandler OnAvailableChanged;

        public static NmcClient Instance { get; private set; }

        [CallPriority(MemberPriority.Highest)]
        public static void Initialize()
        {
            Console.WriteLine("Initializing NmcClient...");
            Instance = new NmcClient();
        }

        public bool Available { get; private set; }

        public NmcClient()
        {
            Timer.DelayCall(CheckInterval, CheckInterval, new TimerCallback(CheckConnection));
        }

        void CheckConnection()
        {
            bool result;

            Debug.Write("Checking RPC Connection: ");
            try
            {
                var svc = new NamecoinLib.Services.NamecoinService();
                var info = svc.GetInfo();
                Debug.WriteLine("Success: Wallet version {0}", info.Version);
                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to connect to Namecoin client: {0}", ex.Message);
                result = false;
            }

            if (result != Available)
            {
                Console.WriteLine("Namecoin client is now {0}.", result ? "online" : "offline");
                Available = result;
                InvokeOnAvailableChanged();
            }

        }

        private void InvokeOnAvailableChanged()
        {
            if (OnAvailableChanged != null)
                OnAvailableChanged(this, Available);
        }
    }
}