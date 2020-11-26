using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 무부하_정상동작.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DcOutCheck_UI : Page, ITestExeList
    {
        public DcOutCheck_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.DcOutCheckVM();
        }
    }
}