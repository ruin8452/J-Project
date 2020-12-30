using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using PropertyChanged;
using System;
using System.Windows;

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

        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand InputQRCommand { get; set; }
        public RelayCommand<object> QRTextChangeCommand { get; set; }
        public RelayCommand CompleteCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        Window window;

        public SerialViewModel()
        {
            Rect = Rectifier.GetObj();
            LoadedCommand = new RelayCommand<object>(SubWinLoad);
            InputQRCommand = new RelayCommand(SerialSave);
            QRTextChangeCommand = new RelayCommand<object>(SubstringSerial);
            CompleteCommand = new RelayCommand(CtrlValueCheck);
            CancelCommand = new RelayCommand(CtrlCancel);
        }

        /**
         *  @brief 확인 버튼 클릭
         *  @details 확인 버튼 클릭 시 합격 처리
         *  
         *  @param
         *  
         *  @return
         */
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

            if (string.IsNullOrEmpty(SerialText))
                return;

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
