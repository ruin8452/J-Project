using Ivi.Visa;
using Ivi.Visa.FormattedIO;
using J_Project.Communication.CommFlags;
using J_Project.Communication.EventArgsClass;
using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace J_Project.Communication.CommModule
{
    public class CommModule_HalfDuplex //: ICommModule
    {
        const double SEND_DELAY = 0.1;
        const double RECIVE_DELAY = 0.05;
        const int MAX_RETRY = 3;

        CommTypeFlag commTypeFlag;
        public bool IsConnected = false;

        readonly object lockObj = 0;

        int token = 0;

        //public event EventHandler ConnectChange;

        // Serial
        SerialPort ComPort;
        bool ComPortReciveFlag = false;
        // USB
        IUsbSession UsbSession;
        MessageBasedFormattedIO FormattedIO;

        /**
         *  @brief 장비 접속
         *  @details VISA ID로 장비에 접속
         *  
         *  @param visaAddress VISA ID
         *  
         *  @return 접속 시도 결과
         */
        public string Connect(string visaAddress)
        {
            try
            {
                UsbSession = GlobalResourceManager.Open(visaAddress) as IUsbSession;
                UsbSession.ServiceRequest += UsbSession_ServiceRequest;

                FormattedIO = new MessageBasedFormattedIO(UsbSession);

                commTypeFlag = CommTypeFlag.USB;
                token = 0;

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
            catch(Exception e)
            {
                return "Connect Error";
            }
        }

        private void UsbSession_ServiceRequest(object sender, VisaEventArgs e)
        {
            throw new NotImplementedException();
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
                    token = 0;

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

        /**
         *  @brief 장비 접속 해제
         *  @details
         *  
         *  @param
         *  
         *  @return 접속 해제 시도 결과
         */
        public TryResultFlag Disconnect()
        {
            if (commTypeFlag == CommTypeFlag.USB)
                UsbSession.Dispose();
            else if (commTypeFlag == CommTypeFlag.SERIAL)
                ComPort.Close();

            return TryResultFlag.SUCCESS;
        }


        /**
         *  @brief 명령 송신
         *  @details 문자열 형식의 명령문 전송
         *  
         *  @param cmd 명령 문자열
         *  
         *  @return 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(string cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                Utill.Delay(SEND_DELAY);

                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                    {
                        token = Thread.CurrentThread.ManagedThreadId;
                        //Debug.WriteLine("send : {0} {1}", token, Thread.CurrentThread.Name);
                    }
                    else
                        continue;
                }

                // 명령 전송
                try
                {

                    if (commTypeFlag == CommTypeFlag.SERIAL)
                    {
                        //ComPort.DiscardInBuffer();  // 버퍼 초기화
                        ComPort.WriteLine(cmd);
                    }
                    else if (commTypeFlag == CommTypeFlag.USB)
                    {
                        //FormattedIO.WriteLine(cmd);

                        UsbSession.ReadStatusByte();
                        UsbSession.FormattedIO.WriteLine(cmd);
                        UsbSession.ReadStatusByte();
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                Utill.Delay(SEND_DELAY);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 송신
         *  @details 바이트 배열 형식의 명령문 전송
         *  
         *  @param cmd 명령 문자열
         *  
         *  @return 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(byte[] cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                Utill.Delay(SEND_DELAY);

                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                    {
                        token = Thread.CurrentThread.ManagedThreadId;
                        //Debug.WriteLine("send : {0} {1}", token, Thread.CurrentThread.Name);
                    }
                    else
                        continue;
                }

                // 명령 전송
                try
                {
                    //ComPort.DiscardInBuffer();  // 버퍼 초기화
                    ComPort.Write(cmd, 0, cmd.Length);
                }
                catch (Exception)
                {
                    continue;
                }
                Utill.Delay(SEND_DELAY);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 수신
         *  @details
         *  
         *  @param
         *  
         *  @return 수신 문자열
         */
        public string CommRecive()
        {
            string result = string.Empty;

            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Utill.Delay(RECIVE_DELAY);
                    continue;
                }

                // 데이터 수신
                if (commTypeFlag == CommTypeFlag.SERIAL)
                {
                    if (ComPortReciveFlag == true)
                    {
                        result = ComPort.ReadLine().Trim('\r', '\n', ' ');
                        ComPortReciveFlag = false;

                        lock (lockObj)
                        {
                            //Debug.WriteLine("recive : {0} {1}", token, Thread.CurrentThread.Name);
                            token = 0;
                        }
                        break;
                    }
                }
                else if (commTypeFlag == CommTypeFlag.USB)
                {
                    try
                    {
                        //result = FormattedIO.ReadLine().Trim('\r', '\n', ' ');

                        UsbSession.ReadStatusByte();
                        result = UsbSession.FormattedIO.ReadLine().Trim('\r', '\n', ' ');
                        UsbSession.ReadStatusByte();

                        lock (lockObj)
                        {
                            //Debug.WriteLine("recive : {0} {1}", token, Thread.CurrentThread.Name);
                            token = 0;
                        }
                        if (!string.IsNullOrEmpty(result))
                            break;
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
                Utill.Delay(RECIVE_DELAY);
            }

            if  (result == "ON") result = "1";
            else if (result == "OFF") result = "0";

            return result;
        }

        /**
         *  @brief 명령 수신
         *  @details
         *  
         *  @param
         *  
         *  @return 수신 바이트배열
         */
        public byte[] CommReciveStream()
        {
            byte[] reciveStream = null;

            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Utill.Delay(RECIVE_DELAY);
                    continue;
                }

                try
                {
                    if (ComPortReciveFlag)
                    {
                        int reciveNum = ComPort.BytesToRead;
                        reciveStream = new byte[reciveNum];

                        ComPort.Read(reciveStream, 0, reciveNum);
                        ComPortReciveFlag = false;
                        break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                Utill.Delay(RECIVE_DELAY);
            }

            lock (lockObj)
            {
                //Debug.WriteLine("recive : {0}", token);
                token = 0;
            }

            return reciveStream;
        }

        public void TokenReset()
        {
            lock(lockObj)
            {
                token = 0;
            }
        }

        /**
         *  @brief RS232 통신 수신 알림
         *  @details RS232 통신 수신 시 수신 플래그 세움
         *  
         *  @param
         *  
         *  @return 수신 바이트배열
         */
        private void CommReciveFlag(object sender, EventArgs e)
        {
            ComPortReciveFlag = true;
        }
    }
}
