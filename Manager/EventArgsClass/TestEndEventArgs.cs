using System;

namespace J_Project.Manager
{
    public class TestEndEventArgs : EventArgs
    {
        public int TestIndex { get; set; }
        public int CaseIndex { get; set; }
        public string Result { get; set; }
        public string Reason { get; set; }

        public TestEndEventArgs(int TestIndex, int CaseIndex, string Result, string Reason)
        {
            this.TestIndex = TestIndex;
            this.CaseIndex = CaseIndex;
            this.Result = Result;
            this.Reason = Reason;
        }
    }
}
