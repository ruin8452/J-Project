using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using PropertyChanged;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace J_Project.ViewModel.SubWindow
{
    public enum LoadCheckMode
    {
        NORMAL,
        OVER_LOAD
    }

    [ImplementPropertyChanged]
    public class LoadCtrlViewModel
    {
        const int MAX_COUNT_DOWN = 1;
        int CountDown;

        public static double TargetValue { get; set; }
        public static double ErrRate { get; set; }
        private static LoadCheckMode CheckMode;

        public static bool CtrlResult = false;
        public Dmm2 Dmm2 { get; set; }
        public Rectifier Rect { get; set; }

        public SolidColorBrush LoadMeterColor { get; set; }

        public string ConfirmBtnText { get; set; }

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand CompleteCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        readonly Timer CountDownTimer = new Timer();
        readonly Timer DmmValueTimer = new Timer();

        Window window;

        public LoadCtrlViewModel()
        {
            LoadMeterColor = Brushes.White;

            CountDownTimer.Interval = 1000;
            CountDownTimer.Tick += CountDownTimer_Tick;

            DmmValueTimer.Interval = 100;
            DmmValueTimer.Tick += DmmValueTimer_Tick;
            DmmValueTimer.Start();

            CountDown = MAX_COUNT_DOWN;

            Dmm2 = Dmm2.GetObj();
            Rect = Rectifier.GetObj();

            ConfirmBtnText = "확인";

            LoadedCommand = new RelayCommand<object>(SubWinLoad);
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
         *  @brief 카운트다운(타이머 이벤트)
         *  @details 일정 시간동안 확인을 누르지 않을 경우 자동 확인
         *  
         *  @param object sender - 이벤트 발생자
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            ConfirmBtnText = string.Format($"확인({CountDown})");
            if (CountDown <= 0)
            {
                CtrlResult = true;
                CountDownTimer.Stop();
                DmmValueTimer.Stop();
                window.Close();
            }
            CountDown--;
        }

        private void DmmValueTimer_Tick(object sender, EventArgs e)
        {
            if(CheckMode == LoadCheckMode.NORMAL)
            {
                if (Math.Abs(TargetValue - Dmm2.DcVolt) <= ErrRate ||
                    Math.Abs(TargetValue - Rect.DcOutputCurr) <= ErrRate)
                {
                    CountDownTimer.Start();

                    CtrlResult = true;
                    LoadMeterColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0xB9, 0x48));
                }
                else
                {
                    CountDownTimer.Stop();
                    CountDown = MAX_COUNT_DOWN;

                    CtrlResult = false;
                    ConfirmBtnText = "확인";
                    LoadMeterColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x43, 0x43));
                }
            }
            else
            {
                if (Rect.Flag_DcOverLoad)
                {
                    CtrlResult = true;
                    DmmValueTimer.Stop();
                    window.Close();
                }
            }
        }

        /**
         *  @brief 부하 설정
         *  @details 부하를 설정하기 위한 데이터를 받아온다
         *  
         *  @param double loadCurr - 설정해야 할 부하값
         *  @param double checkRange - 허용오차
         *  @param LoadCheckMode checkMode - 부하 상태 감지 모드(노멀, 과부하)
         *  
         *  @return
         */
        public static void SetLoadCheck(double loadCurr, double checkRange, LoadCheckMode checkMode)
        {
            TargetValue = loadCurr;
            ErrRate = checkRange;
            CheckMode = checkMode;
        }

        /**
         *  @brief 부하 설정 완료 체크(확인 버튼 클릭)
         *  @details 부하의 설정이 완료됐는지 체크한다
         *  
         *  @param 
         *  
         *  @return
         */
        private void CtrlValueCheck()
        {
            if (CtrlResult == true || Math.Abs(TargetValue - Rect.DcOutputCurr) <= ErrRate)
            {
                CtrlResult = true;
                CountDownTimer.Stop();
                DmmValueTimer.Stop();
                window.Close();
            }
            else
                System.Windows.MessageBox.Show("목표설정값과 현재값이 일치하지 않습니다.");
        }

        /**
         *  @brief 취소 버튼 클릭
         *  @details 취소 버튼 클릭 시 동작
         *  
         *  @param 
         *  
         *  @return
         */
        private void CtrlCancel()
        {
            CtrlResult = false;
            CountDownTimer.Stop();
            DmmValueTimer.Stop();
            window.Close();

        }
    }
}
