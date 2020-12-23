using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 역률.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Cal_DC_출력전류_UI : Page, ITestExeList
    {
        public Cal_DC_출력전류_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.CalDcCurrVM(caseNum);
        }
    }
}