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
    public class LocalSwitchWinViewModel
    {
        public static bool PassOrFail = false; // true : Pass, false : Fail

        public Rectifier Rect { get; set; }

        public ICommand LoadedCommand { get; set; }
        public ICommand EnterCommand { get; set; }
        public ICommand EscCommand { get; set; }

        public ICommand OkCommand { get; set; }
        public ICommand ErrorCommand { get; set; }

        Window window;

        public LocalSwitchWinViewModel()
        {
            Rect = Rectifier.GetObj();

            LoadedCommand = new BaseObjCommand(SubWinLoad);
            EnterCommand = new BaseCommand(Ok);
            EscCommand = new BaseCommand(Error);

            OkCommand = new BaseCommand(Ok);
            ErrorCommand = new BaseCommand(Error);

            //Rect.Connect("COM21", 9600);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        private void Ok()
        {
            PassOrFail = true;
            window.Close();
        }
        private void Error()
        {
            PassOrFail = false;
            window.Close();
        }
    }
}
