using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 무부하_정상동작.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DcOutCheck_Setting_UI : Page, ITestSettingList
    {
        public DcOutCheck_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.DcOutCheckVM(caseNum);
        }
    }
}