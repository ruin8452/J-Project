using System;

namespace J_Project.Manager
{
    public class UnitTestStartEventArgs : EventArgs
    {
        public object TestItem { get; set; }
        public int CurrentSeqNumer { get; set; }
        public int TotalSeqNumer { get; set; }

        public UnitTestStartEventArgs(object testMethod, int currentSeqNumer, int totalSeqNumer)
        {
            TestItem = testMethod;
            CurrentSeqNumer = currentSeqNumer;
            TotalSeqNumer = totalSeqNumer;
        }
    }
}
