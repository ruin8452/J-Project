using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace J_Project.ViewModel.CommandClass
{
    class ComboCommand : ICommand
    {
        Action<string> executeStr;
        Func<bool> _canExecute;

        public ComboCommand(Action<string> execute)
        {
            executeStr = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter == null) return;

            executeStr(parameter.ToString());
        }
    }
}
