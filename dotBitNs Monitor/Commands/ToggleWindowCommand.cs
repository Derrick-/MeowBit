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
    public class ToggleWindowCommand : CommandBase<ToggleWindowCommand>
    {
        public override void Execute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            var imain = win as IMainWindow;
            bool show = !win.IsVisible || win.WindowState == WindowState.Minimized;

            if (show)
            {
                if (imain != null)
                    imain.EnsureVisible();
                else 
                    win.Show();
            }
            else
                win.Hide();

            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}