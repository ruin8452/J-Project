using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace J_Project.Manager
{
    [ImplementPropertyChanged]
    public class TestItemUnit
    {
        public bool? Checked { get; set; }
        public string TestName { get; set; }
        public TestItemUnit Parents { get; set; }
        public ObservableCollection<TestItemUnit> Child { get; set; }
        public Page TestExeUi { get; set; }
        public object remark { get; set; }

        public int TestIndex;
        public int CaseIndex;

        public TestItemUnit()
        {
            Child = new ObservableCollection<TestItemUnit>();
        }
    }
}
