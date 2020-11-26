using System.Windows;

namespace J_Project.UI.SubWindow
{
    /// <summary>
    /// AcCtrlWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TextWindow : Window
    {
        public TextWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel.SubWindow.TextViewModel();
            InputText.Focus();
        }
    }
}
