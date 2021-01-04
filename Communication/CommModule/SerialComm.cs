using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace J_Project.Communication.CommModule
{
    /**
     *  @brief 시리얼 통신 모듈
     *  @details 시리얼 통신을 할때 사용하는 모듈
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    class SerialComm : ICommModule
    {
        const double SEND_DELAY = 0.1;
        const double RECIVE_DELAY = 0.05;
        const int MAX_RETRY = 3;

        SerialPort ComPort;
        bool IsReceive = false;

        readonly object lockObj = 0;

        int token = 0;
        string portName;
        int baudRate;

        /**
         *  @brief 생성자
         *  @details
         *  
         *  @param string portName - 포트번호
         *  @param int baudRate - 통신 속도
         *  
         *  @return
         */
        public SerialComm(string portName, int baudRate)
        {
            this.portName = portName;
            this.baudRate = baudRate;
        }

        /**
         *  @brief 장비 접속
         *  @details Comport로 장비에 접속
         *  
         *  @param 
         *  
         *  @return string - 접속 시도 결과
         */
        public string Connect()
        {
            try
            {
                ComPort = new SerialPort(portName, baudRate);

                ComPort.Open();

                if (ComPort.IsOpen)
                {
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
         *  @return TryResultFlag - 접속 해제 시도 결과
         */
        public TryResultFlag Disconnect()
        {
            try
            {
                if(ComPort != null)
                {
                    ComPort.Close();
                    ComPort.Dispose();
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("ErrPos07" + e.Message);
            }

            return TryResultFlag.SUCCESS;
        }

        /**
         *  @brief 명령 송신
         *  @details 문자열 형식의 명령문 전송
         *  
         *  @param string cmd - 명령 문자열
         *  
         *  @return TryResultFlag - 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(string cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                        token = Thread.CurrentThread.ManagedThreadId;
                    else
                    {
                        Util.Delay(SEND_DELAY);
                        continue;
                    }
                }

                // 명령 전송
                try
                {
                    ComPort.WriteLine(cmd);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos04 : " + e.Message);
                    continue;
                }
                Util.Delay(0.01);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 송신
         *  @details 바이트 배열 형식의 명령문 전송
         *  
         *  @param byte[] cmd - 명령 문자열
         *  
         *  @return TryResultFlag - 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(byte[] cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 할당
                lock (lockObj)
                {
                    Debug.WriteLine("send : " + Thread.CurrentThread.ManagedThreadId);
                    if (token == 0)
                        token = Thread.CurrentThread.ManagedThreadId;
                    else
                    {
                        Debug.WriteLine("송신 Now Token : " + token);
                        Util.Delay(SEND_DELAY);
                        continue;
                    }
                }

                // 명령 전송
                try
                {
                    ComPort.Write(cmd, 0, cmd.Length);
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine("ErrPos08 : " + e.Message);
                    token = 0;
                    Disconnect();
                    Util.Delay(0.5);
                    Connect();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos05 : " + e.Message);
                    token = 0;
                    continue;
                }
                Util.Delay(0.01);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 데이터 수신(문자열)
         *  @details 통신을 통해 데이터를 수신받는 함수
         *
         *  @param out string receiveString - 수신 받은 데이터를 저장할 변수
         *
         *  @return TryResultFlag - 수신 결과
         */
        public TryResultFlag CommReceive(out string receiveString)
        {
            receiveString = string.Empty;

            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Debug.WriteLine("수신 Now Token : " + token);
                    Util.Delay(RECIVE_DELAY);
                    continue;
                }

                // 데이터 수신
                if (IsReceive == true)
                {
                    IsReceive = false;
                    receiveString = ComPort.ReadLine().Trim('\r', '\n', ' ');

                    if (receiveString == "ON") receiveString = "1";
                    else if (receiveString == "OFF") receiveString = "0";

                    lock (lockObj)
                    {
                        token = 0;
                    }

                    return TryResultFlag.SUCCESS;
                }

                Util.Delay(RECIVE_DELAY);
            }
            lock (lockObj)
                token = 0;

            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 데이터 수신(바이트 배열)
         *  @details 통신을 통해 데이터를 수신받는 함수
         *
         *  @param out byte[] receiveString - 수신 받은 데이터를 저장할 변수
         *
         *  @return TryResultFlag - 수신 결과
         */
        public TryResultFlag CommReceive(out byte[] receiveStream)
        {
            receiveStream = null;

            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Util.Delay(RECIVE_DELAY);
                    continue;
                }

                try
                {
                    if (!IsReceive) continue;

                    IsReceive = false;
                    int reciveNum = ComPort.BytesToRead;
                    //receiveStream = new byte[51];
                    receiveStream = new byte[reciveNum];

                    Debug.WriteLine("수신 바이트 : " + reciveNum);

                    //ComPort.Read(receiveStream, 0, 51);
                    ComPort.Read(receiveStream, 0, reciveNum);

                    lock (lockObj)
                    {
                        token = 0;
                    }

                    return TryResultFlag.SUCCESS;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos06 : " + e.Message);
                    token = 0;
                    continue;
                }
            }
            lock (lockObj)
                token = 0;

            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 토큰 반환
         *  @details 통신의 사용하는 토큰을 반환시키는 함수
         *
         *  @param
         *
         *  @return
         */
        public void TokenReset()
        {
            lock (lockObj)
            {
                token = 0;
            }
        }

        /**
         *  @brief 시리얼 통신 수신 이벤트 함수
         *  @details 시리얼 통신 수신 시 수신 플래그 세움
         *  
         *  @param
         *  
         *  @return 수신 바이트배열
         */
        private void CommReciveFlag(object sender, EventArgs e)
        {
            IsReceive = true;
        }
    }
}
