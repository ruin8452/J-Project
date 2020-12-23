using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 온도센서_점검.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class M200Ready_UI : Page, ITestExeList
    {
        public M200Ready_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.M200ReadyVM(caseNum);
        }
    }
}