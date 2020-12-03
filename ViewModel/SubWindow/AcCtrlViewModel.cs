using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using PropertyChanged;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace J_Project.ViewModel.SubWindow
{
    public enum AcCheckMode
    {
        NORMAL,
        AC_OVER,
        AC_UNDER,
        AC_BLACK_OUT
    }

    [ImplementPropertyChanged]
    public class AcCtrlViewModel
    {
        const int MAX_COUNT_DOWN = 1;
        int CountDown;

        public static double TargetValue { get; set; }
        public static double ErrRate { get; set; }
        private static AcCheckMode CheckMode;

        public static bool CtrlResult = false;
        public double AcVolt { get; set; }
        public PowerMeter Pm { get; set; }
        public Rectifier Rect { get; set; }

        public SolidColorBrush AcMeterColor { get; set; }

        public string ConfirmBtnText { get; set; }

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand CompleteCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        readonly Timer CountDownTimer = new Timer();

        Window window;

        public AcCtrlViewModel()
        {
            AcMeterColor = Brushes.White;

            CountDownTimer.Interval = 1000;
            CountDownTimer.Tick += CountDownTimer_Tick;

            Pm = PowerMeter.GetObj();
            Rect = Rectifier.GetObj();

            ConfirmBtnText = "확인";

            Pm.AcVoltRenewal -= AcNormalCheck;
            Pm.AcVoltRenewal -= AcOverCheck;
            Pm.AcVoltRenewal -= AcUnderCheck;
            Pm.AcVoltRenewal -= AcBlackOutCheck;

            if      (CheckMode == AcCheckMode.NORMAL)       Pm.AcVoltRenewal += AcNormalCheck;
            else if (CheckMode == AcCheckMode.AC_OVER)      Pm.AcVoltRenewal += AcOverCheck;
            else if (CheckMode == AcCheckMode.AC_UNDER)     Pm.AcVoltRenewal += AcUnderCheck;
            else if (CheckMode == AcCheckMode.AC_BLACK_OUT) Pm.AcVoltRenewal += AcBlackOutCheck;

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
            if(CountDown <= 0)
            {
                CtrlResult = true;
                CountDownTimer.Stop();
                window.Close();
            }
            CountDown--;
        }

        /**
         *  @brief AC 체크
         *  @details AC가 설정한 값에 근접했는지 체크하고 카운트 다운 활성화
         *  
         *  @param object sender - 이벤트 발생자
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void AcNormalCheck(object sender, EventArgs e)
        {
            if (Math.Abs(Pm.AcVolt - TargetValue) < ErrRate)
            {
                CountDownTimer.Start();

                CtrlResult = true;
                AcMeterColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x4D, 0xB9, 0x48));
            }
            else
            {
                CountDownTimer.Stop();
                CountDown = MAX_COUNT_DOWN;

                ConfirmBtnText = "확인";
                AcMeterColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x43, 0x43));
            }
        }
        /**
         *  @brief AC Over 체크
         *  @details 정류기가 AC 고전압을 인식했을 경우 통과시킨다
         *  
         *  @param object sender - 이벤트 발생자
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void AcOverCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_OverAcInVolt)
            {
                CtrlResult = true;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    window.Close();
                }));
            }
        }
        /**
         *  @brief AC Under 체크
         *  @details 정류기가 AC 저전압을 인식했을 경우 통과시킨다
         *  
         *  @param object sender - 이벤트 발생자
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void AcUnderCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_UnderAcInVolt)
            {
                CtrlResult = true;
                window.Close();
            }
        }
        /**
         *  @brief AC 정전 체크
         *  @details 정류기가 AC 정전을 인식했을 경우 통과시킨다
         *  
         *  @param object sender - 이벤트 발생자
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void AcBlackOutCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_AcRelayOnOff == true)
            {
                CtrlResult = true;
                window.Close();
            }
        }

        /**
         *  @brief AC 설정
         *  @details AC를 설정하기 위한 데이터를 받아온다
         *  
         *  @param double acVolt - 설정해야 할 AC 전압값
         *  @param double checkRange - 허용오차
         *  @param AcCheckMode checkMode - AC 상태 감지 모드(노멀, 저전압, 고전압, 정전)
         *  
         *  @return
         */
        public static void SetAcCheck(double acVolt, double checkRange, AcCheckMode checkMode)
        {
            TargetValue = acVolt;
            ErrRate = checkRange;
            CheckMode = checkMode;
        }

        /**
         *  @brief AC 설정 완료 체크(확인 버튼 클릭)
         *  @details AC의 설정이 완료됐는지 체크한다
         *  
         *  @param 
         *  
         *  @return
         */
        private void CtrlValueCheck()
        {
            if (CtrlResult == true)
            {
                CountDownTimer.Stop();
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
            Pm.AcVoltRenewal -= AcNormalCheck;
            Pm.AcVoltRenewal -= AcOverCheck;
            Pm.AcVoltRenewal -= AcUnderCheck;
            Pm.AcVoltRenewal -= AcBlackOutCheck;

            CountDownTimer.Stop();
            CtrlResult = false;
            window.Close();
        }
    }
}
