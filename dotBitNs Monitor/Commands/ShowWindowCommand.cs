// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 4, 2014

using System.Windows;
using System.Windows.Input;

namespace dotBitNs_Monitor.Commands
{
    /// <summary>
    /// Shows the main window.
    /// </summary>
    public class ShowWindowCommand : CommandBase<ShowWindowCommand>
    {
        public override void Execute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            if (win is IMainWindow)
                ((IMainWindow)win).EnsureVisible();
            else
                win.Show();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            if (win is IMainWindow)
                if (win.WindowState == WindowState.Minimized)
                    return true;
            return win != null && !win.IsVisible;
        }
    }
}