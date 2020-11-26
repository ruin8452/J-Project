using System;

namespace J_Project.Manager.EventArgsClass
{
    public class TestStartEventArgs : EventArgs
    {
        public int TestIndex { get; set; }
        public int CaseIndex { get; set; }

        public TestStartEventArgs(int TestIndex, int CaseIndex)
        {
            this.TestIndex = TestIndex;
            this.CaseIndex = CaseIndex;
        }
    }
}
