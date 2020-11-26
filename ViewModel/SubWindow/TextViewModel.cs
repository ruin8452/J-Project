using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System.Windows;
using System.Windows.Input;

namespace J_Project.ViewModel.SubWindow
{
    [ImplementPropertyChanged]
    public class TextViewModel
    {
        public string InputText { get; set; }
        public static double Number;

        public ICommand LoadedCommand { get; set; }
        public ICommand EnterCommand { get; set; }
        public ICommand CompleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        Window window;

        public TextViewModel()
        {
            LoadedCommand = new BaseObjCommand(SubWinLoad);
            EnterCommand = new BaseObjCommand(InputEnter);
            CompleteCommand = new BaseCommand(CtrlValueCheck);
            CancelCommand = new BaseCommand(CtrlCancel);
        }

        private void SubWinLoad(object obj)
        {
            if (obj is Window window)
                this.window = window;
        }

        private void InputEnter(object text)
        {
            if (!double.TryParse(text.ToString(), out Number))
            {
                MessageBox.Show("올바른 값을 입력해주세요");
                return;
            }

            window.Close();
        }

        private void CtrlValueCheck()
        {
            if (!double.TryParse(InputText, out Number))
            {
                MessageBox.Show("올바른 값을 입력해주세요");
                return;
            }

            window.Close();
        }

        private void CtrlCancel()
        {
            Number = double.NaN;
            window.Close();
        }
    }
}
