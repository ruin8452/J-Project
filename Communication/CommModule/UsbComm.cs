using Ivi.Visa;
using Ivi.Visa.FormattedIO;
using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;
using System.Diagnostics;
using System.Threading;

namespace J_Project.Communication.CommModule
{
    /**
     *  @brief USB 통신 모듈
     *  @details USB 통신을 할때 사용하는 모듈
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class UsbComm : ICommModule
    {
        const double SEND_DELAY = 0.1;
        const double RECIVE_DELAY = 0.05;
        const int MAX_RETRY = 3;

        public bool IsConnected = false;

        readonly object lockObj = 0;

        int token = 0;
        string visaAddress;

        // USB
        IUsbSession UsbSession;
        MessageBasedFormattedIO UsbPort;

        /**
         *  @brief 생성자
         *  @details
         *  
         *  @param string visaAddress - VISA ID
         *  
         *  @return
         */
        public UsbComm(string visaAddress)
        {
            this.visaAddress = visaAddress;
        }

        /**
         *  @brief 장비 접속
         *  @details VISA ID로 장비에 접속
         *  
         *  @param 
         *  
         *  @return string - 접속 시도 결과
         */
        public string Connect()
        {
            try
            {
                UsbSession = GlobalResourceManager.Open(visaAddress) as IUsbSession;
                UsbPort = new MessageBasedFormattedIO(UsbSession);
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
            catch (Exception e)
            {
                Debug.WriteLine("ErrPos01 : " + e.Message);
                return "Connect Error";
            }
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
                if(UsbSession != null)
                    UsbSession.Dispose();
            }
            catch(Exception)
            {

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
                Util.Delay(SEND_DELAY);

                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                    {
                        token = Thread.CurrentThread.ManagedThreadId;
                    }
                    else
                        continue;
                }

                // 명령 전송
                try
                {
                    UsbPort.WriteLine(cmd);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos02 : " + e.Message);
                    token = 0;
                    Disconnect();
                    Connect();
                    continue;
                }
                Util.Delay(SEND_DELAY);

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
                Util.Delay(SEND_DELAY);

                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                        token = Thread.CurrentThread.ManagedThreadId;
                    else
                        continue;
                }

                // 명령 전송
                try
                {
                    UsbPort.WriteBinary(cmd, 0, cmd.Length);
                }
                catch (Exception)
                {
                    continue;
                }
                Util.Delay(SEND_DELAY);

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
                    Util.Delay(RECIVE_DELAY);
                    continue;
                }

                // 데이터 수신
                try
                {
                    receiveString = UsbPort.ReadLine().Trim('\r', '\n', ' ');

                    if (receiveString == "ON") receiveString = "1";
                    else if (receiveString == "OFF") receiveString = "0";

                    lock (lockObj)
                    {
                        token = 0;
                    }

                    return TryResultFlag.SUCCESS;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos03 : " + e.Message);
                    token = 0;
                    continue;
                }
            }

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
                    //receiveStream = new byte[];

                    receiveStream= UsbPort.ReadBinaryBlockOfByte();

                    return TryResultFlag.SUCCESS;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            lock (lockObj)
            {
                token = 0;
            }

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
    }
}
