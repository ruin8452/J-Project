using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// AC_고전압_알람.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 돌입전류_UI : Page, ITestExeList
    {
        public 돌입전류_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.InrushVM(caseNum);
        }
    }
}