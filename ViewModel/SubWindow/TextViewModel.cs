using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Windows;

namespace J_Project.ViewModel.SubWindow
{
    [ImplementPropertyChanged]
    public class TextViewModel
    {
        public string InputText { get; set; }
        public static double Number;

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand<object> EnterCommand { get; set; }
        public RelayCommand CompleteCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        Window window;

        public TextViewModel()
        {
            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            EnterCommand = new RelayCommand<object>(InputEnter);
            CompleteCommand = new RelayCommand(CtrlValueCheck);
            CancelCommand = new RelayCommand(CtrlCancel);
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
         *  @brief 텍스트 입력(단축키 엔터)
         *  @details 사용자가 수동으로 텍스트를 넣고, 올바른 값인지 검사
         *  
         *  @param object text - 사용자가 입력한 텍스트
         *  
         *  @return
         */
        private void InputEnter(object text)
        {
            if (!double.TryParse(text.ToString(), out Number))
            {
                MessageBox.Show("올바른 값을 입력해주세요");
                return;
            }

            window.Close();
        }

        /**
         *  @brief 텍스트 입력(확인 클릭)
         *  @details 사용자가 수동으로 텍스트를 넣고, 올바른 값인지 검사
         *  
         *  @param object text - 사용자가 입력한 텍스트
         *  
         *  @return
         */
        private void CtrlValueCheck()
        {
            if (!double.TryParse(InputText, out Number))
            {
                MessageBox.Show("올바른 값을 입력해주세요");
                return;
            }

            window.Close();
        }

        /**
         *  @brief 취소 버튼 클릭
         *  @details 취소 버튼 클릭 시 데이터 미삽 처리
         *  
         *  @param
         *  
         *  @return
         */
        private void CtrlCancel()
        {
            Number = double.NaN;
            window.Close();
        }
    }
}
