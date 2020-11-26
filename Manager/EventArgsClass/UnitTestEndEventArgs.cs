using System;

namespace J_Project.Manager
{
    public class UnitTestEndEventArgs : EventArgs
    {
        public int TestIndex { get; set; }
        public int CaseIndex { get; set; }
        public int CurrentSeqNumer { get; set; }
        public StateFlag Result { get; set; }

        public UnitTestEndEventArgs(int TestIndex, int CaseIndex, int CurrentSeqNumer, StateFlag Result)
        {
            this.TestIndex = TestIndex;
            this.CaseIndex = CaseIndex;
            this.CurrentSeqNumer = CurrentSeqNumer;
            this.Result = Result;
        }
    }
}
