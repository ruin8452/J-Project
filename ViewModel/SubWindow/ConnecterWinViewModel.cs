using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
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

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand AllCheckCommand { get; set; }
        public RelayCommand OkCommand { get; set; }

        Window window;

        public ConnecterWinViewModel()
        {
            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            AllCheckCommand = new RelayCommand(AllFlagOn);
            OkCommand = new RelayCommand(Ok);
        }

        /**
         *  @brief 윈도우 로드
         *  @details 현재의 서브 윈도우의 객체를 얻는다
         *  
         *  @param object obj - 윈도우 객체
         *  
         *  @return
         */
        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        /**
         *  @brief 플래그 체크
         *  @details 모든 플래그를 true 처리(단축키 A 처리시 동작)
         *  
         *  @param
         *  
         *  @return
         */
        private void AllFlagOn()
        {
            DcOut1Flag = true;
            DcOut2Flag = true;
            DcOut3Flag = true;
            DcOut4Flag = true;
            ParalFlag = true;
            BatFlag = true;
        }

        /**
         *  @brief 확인 버튼 클릭
         *  @details 확인 버튼 클릭시 동작
         *           정상 입력 확인 후 합불 처리
         *  
         *  @param
         *  
         *  @return
         */
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
