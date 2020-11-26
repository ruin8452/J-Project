using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using NPOI.HPSF;
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
    public class ConnecterWinViewModel
    {
        public static bool PassOrFail = false; // true : Pass, false : Fail

        public bool DcOut1Flag { get; set; }
        public bool DcOut2Flag { get; set; }
        public bool DcOut3Flag { get; set; }
        public bool DcOut4Flag { get; set; }
        public bool ParalFlag { get; set; }
        public bool BatFlag { get; set; }

        public ICommand LoadedCommand { get; set; }
        public ICommand AllCheckCommand { get; set; }
        public ICommand OkCommand { get; set; }

        Window window;

        public ConnecterWinViewModel()
        {
            LoadedCommand = new BaseObjCommand(SubWinLoad);
            AllCheckCommand = new BaseCommand(AllFlagOn);
            OkCommand = new BaseCommand(Ok);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        private void AllFlagOn()
        {
            DcOut1Flag = true;
            DcOut2Flag = true;
            DcOut3Flag = true;
            DcOut4Flag = true;
            ParalFlag = true;
            BatFlag = true;
        }

        private void Ok()
        {
            MessageBoxResult result = MessageBox.Show("입력하신 정보가 맞습니까?", "확인", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (DcOut1Flag && DcOut2Flag && DcOut3Flag && DcOut4Flag && ParalFlag && BatFlag)
                    PassOrFail = true;
                else
                    PassOrFail = false;
            }
            else
                return;

            window.Close();
        }
    }
}
