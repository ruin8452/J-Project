using Ivi.Visa.FormattedIO;
using J_Project.Communication.CommFlags;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.Communication.CommModule
{
    public interface ICommModule
    {
        public string Connect();
        public TryResultFlag Disconnect();
        public TryResultFlag CommSend(string cmd);
        public TryResultFlag CommSend(byte[] cmd);
        public TryResultFlag CommReceive(out string receiveString);
        public TryResultFlag CommReceive(out byte[] receiveString);
        public void TokenReset();
    }
}
