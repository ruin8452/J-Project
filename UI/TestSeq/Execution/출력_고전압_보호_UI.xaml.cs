using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 출력_고전압_보호.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 출력_고전압_보호_UI : Page, ITestExeList
    {
        public 출력_고전압_보호_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.OutputHighVM(caseNum);
        }
    }
}