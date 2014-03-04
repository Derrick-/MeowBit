using System.Windows;
using System.Windows.Input;

namespace dotBitNs_Monitor.Commands
{
    /// <summary>
    /// Hides the main window.
    /// </summary>
    public class HideWindowCommand : CommandBase<HideWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Hide();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            if (win is IMainWindow)
                if (win.WindowState == WindowState.Minimized)
                    return false;
            return win != null && win.IsVisible;
        }
    }
}