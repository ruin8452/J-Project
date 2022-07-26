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
    public enum AlarmCheckMode
    {
        AC_OVER,
        AC_UNDER,
        AC_BLACK_OUT
    }

    [ImplementPropertyChanged]
    public class RectAcViewModel
    {
        public static double TargetValue { get; set; }
        private static AlarmCheckMode CheckMode;

        public static bool CtrlResult = false;
        public Rectifier Rect { get; set; }

        public SolidColorBrush AcMeterColor { get; set; }

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand CompleteCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        readonly Timer RectValueTimer = new Timer();

        Window window;

        public RectAcViewModel()
        {
            AcMeterColor = Brushes.White;

            RectValueTimer.Interval = 100;
            RectValueTimer.Tick += RectValueTimer_Tick;
            RectValueTimer.Start();

            Rect = Rectifier.GetObj();

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

            CtrlResult = false;
        }

        private void RectValueTimer_Tick(object sender, EventArgs e)
        {
            if (CheckMode == AlarmCheckMode.AC_OVER)
            {
                if (Rect.Flag_OverAcInVolt)
                {
                    CtrlResult = true;
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        RectValueTimer.Stop();
                        window.Close();
                    }));
                }
            }
            else if (CheckMode == AlarmCheckMode.AC_UNDER)
            {
                if (Rect.Flag_UnderAcInVolt)
                {
                    CtrlResult = true;
                    RectValueTimer.Stop();
                    window.Close();
                }
            }
            else
            {
                if (Rect.Flag_AcRelayOnOff == true)
                {
                    CtrlResult = true;
                    RectValueTimer.Stop();
                    window.Close();
                }
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
        public static void SetAcCheck(double acVolt, AlarmCheckMode checkMode)
        {
            TargetValue = acVolt;
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
                RectValueTimer.Stop();
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
            RectValueTimer.Stop();
            CtrlResult = false;
            window.Close();
        }
    }
}
