using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// DcOnCheck_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConnecterCheck_UI : Page
    {
        public ConnecterCheck_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.ConnecterVM(caseNum);
        }
    }
}
