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
    public class LedCheckWinViewModel
    {
        const double LED_DELAY = 0.2;

        public static bool PassOrFail = false; // true : Pass, false : Fail

        public bool RedLightFlag { get; set; }
        public bool RedBlinkFlag { get; set; }
        public bool YellowLightFlag { get; set; }
        public bool YellowBlinkFlag { get; set; }
        public bool GreenLightFlag { get; set; }
        public bool GreenBlinkFlag { get; set; }

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand EnterCommand { get; set; }
        public RelayCommand EscCommand { get; set; }

        public RelayCommand RedLightCommand { get; set; }
        public RelayCommand RedBlinkCommand { get; set; }
        public RelayCommand YellowLightCommand { get; set; }
        public RelayCommand YellowBlinkCommand { get; set; }
        public RelayCommand GreenLightCommand { get; set; }

        public RelayCommand CheckCommand { get; set; }
        public RelayCommand OkCommand { get; set; }
        public RelayCommand ErrorCommand { get; set; }

        Window window;

        public LedCheckWinViewModel()
        {
            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            EnterCommand = new RelayCommand(Ok);
            EscCommand = new RelayCommand(Error);

            RedLightCommand = new RelayCommand(RedLight);
            RedBlinkCommand = new RelayCommand(RedBlink);
            YellowLightCommand = new RelayCommand(YellowLight);
            YellowBlinkCommand = new RelayCommand(YellowBlink);
            GreenLightCommand = new RelayCommand(GreenLight);

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
         *  @brief 적색 점등
         *  @details 정류기에게 LED 적색 점등 명령 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void RedLight()
        {
            Rectifier.GetObj().MonitoringStop();

            if (Rectifier.GetObj().Flag_AcFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL2, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if(Rectifier.GetObj().Flag_BatFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL3, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if(Rectifier.GetObj().Flag_BatEx == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL4, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatEx == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            ///////
            if(Rectifier.GetObj().Flag_RectFail == false)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL1, 1);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_RectFail == true)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            Rectifier.GetObj().MonitoringStart();
        }
        /**
         *  @brief 적색 점멸
         *  @details 정류기에게 LED 적색 점멸 명령 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void RedBlink()
        {
            Rectifier.GetObj().MonitoringStop();

            if (Rectifier.GetObj().Flag_RectFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL1, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_RectFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL3, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatEx == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL4, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            ///////
            if(Rectifier.GetObj().Flag_AcFail == false)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL2, 1);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == true)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            Rectifier.GetObj().MonitoringStart();
        }
        /**
         *  @brief 황색 점등
         *  @details 정류기에게 LED 황색 점등 명령 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void YellowLight()
        {
            Rectifier.GetObj().MonitoringStop();

            if (Rectifier.GetObj().Flag_RectFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL1, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_RectFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_AcFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL2, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatEx == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL4, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatEx == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            ///////
            if (Rectifier.GetObj().Flag_BatFail == false)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL3, 1);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatFail == true)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            Rectifier.GetObj().MonitoringStart();
        }
        /**
         *  @brief 황색 점멸
         *  @details 정류기에게 LED 황색 점멸 명령 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void YellowBlink()
        {
            Rectifier.GetObj().MonitoringStop();

            if (Rectifier.GetObj().Flag_RectFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL1, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_RectFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_AcFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL2, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL3, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            ///////
            if (Rectifier.GetObj().Flag_BatEx == false)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL4, 1);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatEx == true)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            Rectifier.GetObj().MonitoringStart();
        }
        /**
         *  @brief 녹색 점등
         *  @details 정류기에게 LED 녹색 점등 명령 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void GreenLight()
        {
            Rectifier.GetObj().MonitoringStop();

            if (Rectifier.GetObj().Flag_RectFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL1, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_RectFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_AcFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL2, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_AcFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatFail == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL3, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatFail == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }
            if (Rectifier.GetObj().Flag_BatEx == true)
            {
                for (int loop = 0; loop < 5; loop++)
                {
                    Rectifier.GetObj().RectCommand(CommandList.HW_FAIL4, 0);
                    Rectifier.GetObj().RectMonitoring();
                    if (Rectifier.GetObj().Flag_BatEx == false)
                        break;
                    Manager.Util.Delay(LED_DELAY);
                }
            }

            Rectifier.GetObj().MonitoringStart();
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

            MessageBoxResult result = MessageBox.Show("12V 전원을 꺼주세요.", "12V 전원 OFF", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;

            GreenLight();
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

            GreenLight();
            window.Close();
        }
    }
}
