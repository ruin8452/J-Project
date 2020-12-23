using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 잡음전압.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 리플_노이즈_UI : Page, ITestExeList
    {
        public 리플_노이즈_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.NoiseVM(caseNum);
        }
        public 리플_노이즈_UI(object dataContext)
        {
            InitializeComponent();

            DataContext = dataContext;
        }
    }
}