using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 온도센서_점검.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 온도센서_점검_UI : Page, ITestExeList
    {
        public 온도센서_점검_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.TempVM(caseNum);
        }
    }
}