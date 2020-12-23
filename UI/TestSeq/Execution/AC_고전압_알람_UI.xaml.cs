using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// AC_고전압_알람.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AC_고전압_알람_UI : Page, ITestExeList
    {
        public AC_고전압_알람_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.AcHighVM(caseNum);
        }
    }
}