using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// AC_정전_인식.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AC_정전전압_인식_Setting_UI : Page, ITestSettingList
    {
        public AC_정전전압_인식_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.AcBlackOutVM(caseNum);
        }
    }
}