using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.SubWindow
{
    [ImplementPropertyChanged]
    public class SerialViewModel
    {
        public string QRText { get; set; }
        public string SerialText { get; set; }
        public string ReadText { get; set; }

        public Rectifier Rect { get; set; }

        public static bool CtrlResult = false;

        public ICommand LoadedCommand { get; set; }
        public ICommand InputQRCommand { get; set; }
        public ICommand QRTextChangeCommand { get; set; }
        public ICommand CompleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        Window window;

        public SerialViewModel()
        {
            Rect = Rectifier.GetObj();
            LoadedCommand = new BaseObjCommand(SubWinLoad);
            InputQRCommand = new BaseCommand(SerialSave);
            QRTextChangeCommand = new BaseObjCommand(SubstringSerial);
            CompleteCommand = new BaseCommand(CtrlValueCheck);
            CancelCommand = new BaseCommand(CtrlCancel);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        private void SubstringSerial(object text)
        {
            string inputText = text.ToString();

            try
            {
                SerialText = inputText.Substring(inputText.Length - 5);
            }
            catch (Exception)
            {
                SerialText = inputText;
            }
        }

        private void SerialSave()
        {
            int time = 20;

            Rect.RectCommand(CommandList.EEPROM_WRITE, 130, ushort.Parse(SerialText));
            Rect.RectCommand(CommandList.EEPROM_READ, 130);

            Util.Delay(1);

            ReadText = Rect.EEPRomData.ToString();

            while(time != 0)
            {
                Util.Delay(0.5);
                if (Rect.EEPRomData != 0)
                {
                    ReadText = Rect.EEPRomData.ToString();
                    break;
                }
                time--;
            }
        }

        private void CtrlValueCheck()
        {
            CtrlResult = true;
            window.Close();
        }

        private void CtrlCancel()
        {
            CtrlResult = false;
            window.Close();
        }
    }
}
