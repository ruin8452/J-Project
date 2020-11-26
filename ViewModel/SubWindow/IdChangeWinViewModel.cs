using J_Project.Communication.CommModule;
using J_Project.Equipment;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace J_Project.ViewModel.SubWindow
{
    [ImplementPropertyChanged]
    class IdChangeWinViewModel
    {
        public static bool PassOrFail = false; // true : Pass, false : Fail

        public Rectifier Rect { get; set; }

        public ICommand LoadedCommand { get; set; }
        public ICommand EnterCommand { get; set; }
        public ICommand EscCommand { get; set; }

        public ICommand OkCommand { get; set; }
        public ICommand ErrorCommand { get; set; }

        Window window;

        public IdChangeWinViewModel()
        {
            Rect = Rectifier.GetObj();
            EnterCommand = new BaseCommand(Ok);
            EscCommand = new BaseCommand(Error);

            LoadedCommand = new BaseObjCommand(SubWinLoad);
            OkCommand = new BaseCommand(Ok);
            ErrorCommand = new BaseCommand(Error);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }
        private void Ok()
        {
            window.DialogResult = true;
            PassOrFail = true;
            window.Close();
        }
        private void Error()
        {
            window.DialogResult = false;
            PassOrFail = false;
            window.Close();
        }
    }
}
