using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 로드_레귤레이션.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 레귤레이션_100V_Setting_UI : Page, ITestSettingList
    {
        public 레귤레이션_100V_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.RegulM100VM(caseNum);
        }
    }
}