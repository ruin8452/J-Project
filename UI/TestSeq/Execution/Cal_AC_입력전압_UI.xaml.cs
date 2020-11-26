using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 역률.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Cal_AC_입력전압_UI : Page, ITestExeList
    {
        public Cal_AC_입력전압_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.CalAcVM();
        }
    }
}