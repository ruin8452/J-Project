using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using PropertyChanged;
using System.Windows;

namespace J_Project.ViewModel.SubWindow
{
    [ImplementPropertyChanged]
    public class LocalSwitchWinViewModel
    {
        public static bool PassOrFail = false; // true : Pass, false : Fail

        public Rectifier Rect { get; set; }

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand EnterCommand { get; set; }
        public RelayCommand EscCommand { get; set; }

        public RelayCommand OkCommand { get; set; }
        public RelayCommand ErrorCommand { get; set; }

        Window window;

        public LocalSwitchWinViewModel()
        {
            Rect = Rectifier.GetObj();

            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            EnterCommand = new RelayCommand(Ok);
            EscCommand = new RelayCommand(Error);

            OkCommand = new RelayCommand(Ok);
            ErrorCommand = new RelayCommand(Error);

            //Rect.Connect("COM21", 9600);
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
            PassOrFail = false;
            window.Close();
        }
    }
}
