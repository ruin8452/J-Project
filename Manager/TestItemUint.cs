using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace J_Project.Manager
{
    [ImplementPropertyChanged]
    public class TestItemUint
    {
        public bool? Checked { get; set; }
        public string TestName { get; set; }
        public TestItemUint Parents { get; set; }
        public ObservableCollection<TestItemUint> Child { get; set; }
        public Page TestExeUi { get; set; }

        public int TestIndex;
        public int CaseIndex;

        public TestItemUint()
        {
            Child = new ObservableCollection<TestItemUint>();
        }
    }
}
