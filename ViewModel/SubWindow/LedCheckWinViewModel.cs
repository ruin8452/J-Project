using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
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

        public ICommand LoadedCommand { get; set; }
        public ICommand EnterCommand { get; set; }
        public ICommand EscCommand { get; set; }

        public ICommand RedLightCommand { get; set; }
        public ICommand RedBlinkCommand { get; set; }
        public ICommand YellowLightCommand { get; set; }
        public ICommand YellowBlinkCommand { get; set; }
        public ICommand GreenLightCommand { get; set; }

        public ICommand CheckCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand ErrorCommand { get; set; }

        Window window;

        public LedCheckWinViewModel()
        {
            LoadedCommand = new BaseObjCommand(SubWinLoad);
            EnterCommand = new BaseCommand(Ok);
            EscCommand = new BaseCommand(Error);

            RedLightCommand = new BaseCommand(RedLight);
            RedBlinkCommand = new BaseCommand(RedBlink);
            YellowLightCommand = new BaseCommand(YellowLight);
            YellowBlinkCommand = new BaseCommand(YellowBlink);
            GreenLightCommand = new BaseCommand(GreenLight);

            OkCommand = new BaseCommand(Ok);
            ErrorCommand = new BaseCommand(Error);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

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

        private void Ok()
        {
            PassOrFail = true;

            MessageBoxResult result = MessageBox.Show("12V 전원을 꺼주세요.", "12V 전원 OFF", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;

            GreenLight();
            window.Close();
        }
        private void Error()
        {
            PassOrFail = false;

            GreenLight();
            window.Close();
        }
    }
}
