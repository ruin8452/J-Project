using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace J_Project.Manager
{
    [ImplementPropertyChanged]
    class TestItemSetUnit
    {
        public string TestName { get; set; }
        public Page TestSetUi { get; set; }

        public TestItemSetUnit(string testName, Page testSetUi)
        {
            TestName = testName;
            TestSetUi = testSetUi;
        }

        public override string ToString()
        {
            return TestName;
        }
    }
}
