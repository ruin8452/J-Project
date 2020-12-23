using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 라인_레귤레이션_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 레귤레이션_200V_UI : Page, ITestExeList
    {
        public 레귤레이션_200V_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.RegulM200VM(caseNum);
        }
    }
}