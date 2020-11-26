using System;

namespace J_Project.Manager
{
    public class TestRunCheckEventArgs : EventArgs
    {
        public bool RunState { get; set; }

        public TestRunCheckEventArgs(bool RunState)
        {
            this.RunState = RunState;
        }
    }
}
