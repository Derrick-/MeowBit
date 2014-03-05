// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs_Monitor;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace dotBitNs_Monitor.Commands
{
    /// <summary>
    /// Closes the current window.
    /// </summary>
    public class OpenBrowserCommand : CommandBase<OpenBrowserCommand>
    {
        static string[] validSchemes = new string[] { "http", "https" };
        public override void Execute(object parameter)
        {
            string url = parameter as string;
            Uri uri;
            if (url != null && Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (validSchemes.Contains(uri.Scheme))
                    Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
            }
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            string url = parameter as string;
            Uri uri;
            return url != null && Uri.TryCreate(url, UriKind.Absolute, out uri);
        }
    }
}