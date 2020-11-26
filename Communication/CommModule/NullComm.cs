using J_Project.Communication.CommFlags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.Communication.CommModule
{
    /**
     *  @brief 빈 통신 모듈
     *  @details 동작하지 않는 빈 통신 모듈(= NULL)
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    class NullComm : ICommModule
    {
        /**
         *  @brief 접속
         *  @details 장비와 통신 연결하는 함수
         *
         *  @param 
         *
         *  @return string - 연결 결과
         */
        public string Connect()
        {
            return "Couldn't Connected!";
        }

        /**
         *  @brief 접속 해제
         *  @details 장비와 통신 연결을 해제하는 함수
         *
         *  @param 
         *
         *  @return TryResultFlag - 해제 결과
         */
        public TryResultFlag Disconnect()
        {
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
            receiveString = null;
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
        public TryResultFlag CommReceive(out byte[] receiveString)
        {
            receiveString = null;
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 데이터 송신(문자열)
         *  @details 통신을 통해 명령을 송신하는 함수
         *
         *  @param string cmd - 송신할 명령을 담은 변수
         *
         *  @return TryResultFlag - 송신 결과
         */
        public TryResultFlag CommSend(string cmd)
        {
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 데이터 송신(바이트 배열)
         *  @details 통신을 통해 명령을 송신하는 함수
         *
         *  @param byte[] cmd - 송신할 명령을 담은 변수
         *
         *  @return TryResultFlag - 송신 결과
         */
        public TryResultFlag CommSend(byte[] cmd)
        {
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
            
        }
    }
}
