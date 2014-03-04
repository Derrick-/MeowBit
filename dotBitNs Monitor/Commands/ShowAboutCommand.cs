using dotBitNs_Monitor;
using System.Windows;
using System.Windows.Input;

namespace dotBitNs_Monitor.Commands
{
    public class ShowAboutCommand : CommandBase<ShowAboutCommand>
    {
        public override void Execute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            if (win is IMainWindow)
                ((IMainWindow)win).ShowAbout();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}