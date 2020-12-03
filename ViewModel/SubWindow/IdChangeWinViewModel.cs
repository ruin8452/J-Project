using GalaSoft.MvvmLight.Command;
using J_Project.Communication.CommModule;
using J_Project.Equipment;
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

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand EnterCommand { get; set; }
        public RelayCommand EscCommand { get; set; }

        public RelayCommand OkCommand { get; set; }
        public RelayCommand ErrorCommand { get; set; }

        Window window;

        public IdChangeWinViewModel()
        {
            Rect = Rectifier.GetObj();
            EnterCommand = new RelayCommand(Ok);
            EscCommand = new RelayCommand(Error);

            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            OkCommand = new RelayCommand(Ok);
            ErrorCommand = new RelayCommand(Error);
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
         *  @brief 확인 버튼 클릭
         *  @details 확인 버튼 클릭 시 합격 처리
         *  
         *  @param
         *  
         *  @return
         */
        private void Ok()
        {
            window.DialogResult = true;
            PassOrFail = true;
            window.Close();
        }
        /**
         *  @brief 취소 버튼 클릭
         *  @details 취소 버튼 클릭 시 불합격 처리
         *  
         *  @param
         *  
         *  @return
         */
        private void Error()
        {
            window.DialogResult = false;
            PassOrFail = false;
            window.Close();
        }
    }
}
