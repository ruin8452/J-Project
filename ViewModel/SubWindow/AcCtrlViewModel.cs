using J_Project.Equipment;
using J_Project.ViewModel.CommandClass;
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

        public ICommand LoadedCommand { get; set; }
        public ICommand CompleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

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

            LoadedCommand = new BaseObjCommand(SubWinLoad);
            CompleteCommand = new BaseCommand(CtrlValueCheck);
            CancelCommand = new BaseCommand(CtrlCancel);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

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

        // AC 체크
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
        // AC Over 체크
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
        // AC Under 체크
        private void AcUnderCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_UnderAcInVolt)
            {
                CtrlResult = true;
                window.Close();
            }
        }
        // AC 정전 체크
        private void AcBlackOutCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_AcRelayOnOff == true)
            {
                CtrlResult = true;
                window.Close();
            }
        }

        public static void SetAcCheck(double acVolt, double checkRange, AcCheckMode checkMode)
        {
            TargetValue = acVolt;
            ErrRate = checkRange;
            CheckMode = checkMode;
        }

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
