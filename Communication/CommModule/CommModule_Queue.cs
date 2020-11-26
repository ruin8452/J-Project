using Ivi.Visa;
using Ivi.Visa.FormattedIO;
using J_Project.Communication.CommFlags;
using J_Project.Communication.EventArgsClass;
using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace J_Project.Communication.CommModule
{
    public class CommModule_Queue : ICommModule
    {
        const int RECIVE_DELAY = 10;
        const int RECIVE_TRY_COUNT = 5;

        CommTypeFlag commTypeFlag;
        public bool IsConnected = false;
        Queue<string> SendQueue = new Queue<string>();
        Queue<string> ReciveQueue = new Queue<string>();
        private DispatcherTimer Timer = new DispatcherTimer();


        //public event EventHandler ConnectChange;

        // Serial
        SerialPort ComPort;
        bool ComPortReciveFlag = false;
        // USB
        IMessageBasedSession Session;
        MessageBasedFormattedIO FormattedIO;

        public CommModule_Queue()
        {
            Timer.Interval = TimeSpan.FromMilliseconds(10);
            Timer.Tick += new EventHandler(SendCmd_Tick);
        }


        // VISA ID로 접속
        public string Connect(string visaAddress)
        {
            try
            {
                Session = GlobalResourceManager.Open(visaAddress) as IMessageBasedSession;
                FormattedIO = new MessageBasedFormattedIO(Session);

                commTypeFlag = CommTypeFlag.USB;

                return "Connected!";
            }
            catch (NativeVisaException)
            {
                return "Couldn't Connected!";
            }
            catch (ArgumentException)
            {
                return "Invalid argument";
            }
        }

        // Comport로 접속
        public string Connect(string portName, int baudRate)
        {
            try
            {
                ComPort = new SerialPort(portName, baudRate);

                ComPort.Open();

                if (ComPort.IsOpen)
                {
                    commTypeFlag = CommTypeFlag.SERIAL;
                    ComPort.DataReceived += CommReciveFlag;

                    return "Connected!";
                }
                else
                    ComPort.Close();
            }
            catch (IOException)
            {
                ComPort.Close();
                return "Not Find Port";
            }
            catch (UnauthorizedAccessException)
            {
                ComPort.Close();
                return "Alread Using Port";
            }
            catch (InvalidOperationException)
            {
                ComPort.Close();
                return "Alread Open Port";
            }
            catch (ArgumentException)
            {
                ComPort.Close();
                return "Wrong Port Name";
            }

            return "Couldn't Connected!";
        }
        public TryResultFlag Disconnect()
        {
            if (commTypeFlag == CommTypeFlag.USB)
                Session.Dispose();
            else if (commTypeFlag == CommTypeFlag.SERIAL)
                ComPort.Close();

            return TryResultFlag.SUCCESS;
        }


        public TryResultFlag CommSend(string cmd)
        {
            SendQueue.Enqueue(cmd);
            return TryResultFlag.SUCCESS;
        }
        public string CommRecive()
        {
            string result = string.Empty;

            for (int i = 0; i < RECIVE_TRY_COUNT; i++)
            {
                if (commTypeFlag == CommTypeFlag.SERIAL)
                {
                    if (ComPortReciveFlag == true)
                    {
                        result = ComPort.ReadLine().Trim('\r', '\n', ' ');
                        ComPortReciveFlag = false;
                        break;
                    }
                }
                else if (commTypeFlag == CommTypeFlag.USB)
                {
                    try
                    {
                        result = FormattedIO.ReadLine().Trim('\r', '\n', ' ');
                    }
                    catch (Exception)
                    { }

                    if (!string.IsNullOrEmpty(result)) break;
                }

                Utill.Delay(RECIVE_DELAY);
            }

            if (result == "ON") result = "1";
            else if (result == "OFF") result = "0";

            ReciveQueue.Enqueue(result);

            return result;
        }

        public TryResultFlag CommSend(byte[] cmd)
        {
            throw new NotImplementedException();
        }

        public byte[] CommReciveStream()
        {
            byte[] reciveStream = Array.Empty<byte>();

            for (int i = 0; i < RECIVE_TRY_COUNT; i++)
            {
                if (ComPortReciveFlag)
                {
                    int reciveNum = ComPort.BytesToRead;
                    reciveStream = new byte[reciveNum];

                    ComPort.Read(reciveStream, 0, reciveNum);
                    ComPortReciveFlag = false;
                    break;
                }

                Utill.Delay(RECIVE_DELAY);
            }

            return reciveStream;
        }

        private void CommReciveFlag(object sender, EventArgs e)
        {
            ComPortReciveFlag = true;
        }
        private void SendCmd_Tick(object sender, EventArgs e)
        {
            // 수신 받은 데이터를 가져가지 않았으면 대기
            if (ComPortReciveFlag == true) return;

            if (commTypeFlag == CommTypeFlag.SERIAL)
                ComPort.WriteLine(SendQueue.Dequeue());
            else if (commTypeFlag == CommTypeFlag.USB)
                FormattedIO.WriteLine(SendQueue.Dequeue());
        }

        public void TokenReset()
        {
            return;
        }
    }
}