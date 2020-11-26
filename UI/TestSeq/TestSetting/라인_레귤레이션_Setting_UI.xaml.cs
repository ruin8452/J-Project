using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 라인_레귤레이션_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 라인_레귤레이션_Setting_UI : Page, ITestSettingList
    {
        public 라인_레귤레이션_Setting_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.LineRegVM();
        }
    }
}