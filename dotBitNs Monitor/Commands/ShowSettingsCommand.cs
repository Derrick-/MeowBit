// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using dotBitNs_Monitor;
using System.Windows;
using System.Windows.Input;

namespace dotBitNs_Monitor.Commands
{
    /// <summary>
    /// Closes the current window.
    /// </summary>
    public class ShowSettingsCommand : CommandBase<ShowSettingsCommand>
    {
        public override void Execute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            if (win is IMainWindow)
                ((IMainWindow)win).ShowSettingsWindow(SettingsWindow.TabName.Settings);
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}