using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 역률.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 역률_UI : Page, ITestExeList
    {
        public 역률_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.PowerFactorVM(caseNum);
        }
        public 역률_UI(object dataContext)
        {
            InitializeComponent();

            DataContext = dataContext;
        }
    }
}