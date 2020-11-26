using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.TestMethod.EventArgsClass
{
    public class ResultCheckEventArgs : EventArgs
    {
        public string Result { get; set; }

        public ResultCheckEventArgs(string result)
        {
            Result = result;
        }
    }
}
