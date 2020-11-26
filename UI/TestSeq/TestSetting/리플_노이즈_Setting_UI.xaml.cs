using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 잡음전압.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 리플_노이즈_Setting_UI : Page, ITestSettingList
    {
        public 리플_노이즈_Setting_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.NoiseVM();
        }
    }
}