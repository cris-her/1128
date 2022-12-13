
namespace rfid1128.Infrastructure
{
    /// <summary>
    /// Extension methods for commands.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Assumes command is a <see cref="RelayCommand"/> and calls <see cref="RelayCommand.RaiseCanExecuteChanged"/>
        /// </summary>
        /// <param name="command">The command to raise <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/></param>
        public static void RefreshCanExecute(this System.Windows.Input.ICommand command)
        {
            (command as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
