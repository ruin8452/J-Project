using J_Project.Equipment;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.SubWindow
{
    public enum LoadCheckMode
    {
        NORMAL,
        OVER_LOAD,
        SECOND_TEST
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

        public ICommand LoadedCommand { get; set; }
        public ICommand CompleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        readonly Timer CountDownTimer = new Timer();

        Window window;

        public LoadCtrlViewModel()
        {
            LoadMeterColor = Brushes.White;

            CountDownTimer.Interval = 1000;
            CountDownTimer.Tick += CountDownTimer_Tick;

            CountDown = MAX_COUNT_DOWN;

            Dmm2 = Dmm2.GetObj();
            Rect = Rectifier.GetObj();

            Dmm2.DcVoltRenewal -= LoadNormalCheck;
            Dmm2.DcVoltRenewal -= LoadOverCheck;
            Rect.MonitorRenewal -= RectNormalCheck;

            if (CheckMode == LoadCheckMode.NORMAL) Dmm2.DcVoltRenewal += LoadNormalCheck;
            else if (CheckMode == LoadCheckMode.OVER_LOAD) Dmm2.DcVoltRenewal += LoadOverCheck;
            //else if (CheckMode == LoadCheckMode.SECOND_TEST) Rect.MonitorRenewal += RectNormalCheck;

            ConfirmBtnText = "확인";

            LoadedCommand = new BaseObjCommand(SubWinLoad);
            CompleteCommand = new BaseCommand(CtrlValueCheck);
            CancelCommand = new BaseCommand(CtrlCancel);
        }

        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            ConfirmBtnText = string.Format($"확인({CountDown})");
            if (CountDown <= 0)
            {
                CtrlResult = true;
                CountDownTimer.Stop();
                window.Close();
            }
            CountDown--;
        }

        private void LoadNormalCheck(object sender, EventArgs e)
        {
            if (Math.Abs(TargetValue - Dmm2.DcVolt) <= ErrRate)
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
        // Over Load 체크
        private void LoadOverCheck(object sender, EventArgs e)
        {
            if (Rect.Flag_DcOverLoad)
            {
                CtrlResult = true;
                window.Close();
            }
        }

        private void RectNormalCheck(object sender, EventArgs e)
        {
            if (Math.Abs(TargetValue - Rect.DcOutputCurr) <= ErrRate)
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

        public static void SetLoadCheck(double loadCurr, double checkRange, LoadCheckMode checkMode)
        {
            TargetValue = loadCurr;
            ErrRate = checkRange;
            CheckMode = checkMode;
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        private void CtrlValueCheck()
        {
            if (CtrlResult == true || Math.Abs(TargetValue - Rect.DcOutputCurr) <= ErrRate)
            {
                CtrlResult = true;
                CountDownTimer.Stop();
                window.Close();
            }
            else
                System.Windows.MessageBox.Show("목표설정값과 현재값이 일치하지 않습니다.");
        }

        private void CtrlCancel()
        {
            CtrlResult = false;
            CountDownTimer.Stop();
            window.Close();

        }
    }
}
