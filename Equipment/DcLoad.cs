using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;

namespace J_Project.Equipment
{
    /**
     *  @brief DC 전자 부하 장비
     *  @details DC 전자 부하에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class DcLoad : Equipment
    {
        #region 싱글톤 패턴 구현
        private static DcLoad _DcLoad = null;

        private DcLoad()
        {
            EquiName = "부하";
            //CommModule = new CommModule_HalfDuplex();
            //CommModule.ConnectChange += new EventHandler(ConnectChagne);

            //EquiMonitoring.Interval = TimeSpan.FromMilliseconds(1000);
            //EquiMonitoring.Tick += RectMonitoring;
        }

        public static DcLoad GetObj()
        {
            if (_DcLoad == null) _DcLoad = new DcLoad();
            return _DcLoad;
        }
        #endregion

        /**
         *  @brief 원격 조작 명령
         *  @details 명령을 하기 전에 원격 조작을 수행하겠다는 명령
         *  
         *  @param 
         *  
         *  @return 1 - 원격 조작 명령 전송
         */
        public int Remote()
        {
            CommModule.CommSend("SYSTem:REMote");
            CommModule.TokenReset();

            return 1;
        }

        /**
         *  @brief 부하 전압 설정
         *  @details DC 전자 부하 장비의 전압값을 설정한다.
         *  
         *  @param double acv - 설정할 부하 전압값
         *  
         *  @return double - 현재 DC 전자 부하에 설정되어 있는 전압 값
         */
        public double VoltSet(double volt)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("VOLTage " + volt);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("VOLTage?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            CommErrCount = 0;
            if (string.IsNullOrEmpty(resultStr))
                return double.NaN;
            else
                return Convert.ToDouble(resultStr, Util.Cultur);
        }

        /**
         *  @brief 부하 전류 설정
         *  @details DC 전자 부하 장비의 전류값을 설정한다.
         *  
         *  @param double curr - 설정할 부하 전류값
         *  
         *  @return double - 현재 DC 전자 부하에 설정되어 있는 전류 값
         */
        public double CurrSet(double curr)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CURRent " + curr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("CURRent?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            CommErrCount = 0;
            if (string.IsNullOrEmpty(resultStr))
                return double.NaN;
            else
                return Convert.ToDouble(resultStr, Util.Cultur);
        }

        /**
         *  @brief 부하 출력 설정
         *  @details 부하의 출력을 설정한다.
         *  
         *  @param CtrlFlag flag - 출력 여부
         *  @li OFF : 출력 OFF
         *  @li ON : 출력 ON
         *  
         *  @return CtrlFlag? - 현재 AC 소스의 출력 여부
         *  @li OFF : 현재 출력 OFF 상태
         *  @li ON : 현재 출력 ON 상태
         *  @li NULL : 수신 에러
         */
        public CtrlFlag? LoadPowerCtrl(CtrlFlag flag)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("INPut " + flag);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("INPut?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return (CtrlFlag?)Convert.ToInt32(resultStr, Util.Cultur);
        }

        /**
         *  @brief 실시간 전압 값
         *  @details 부하의 실시간 전압 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 부하가 출력 중인 전압 값
         */
        public double RealLoadVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:VOLTage?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            CommErrCount = 0;
            if (string.IsNullOrEmpty(resultStr))
                return double.NaN;
            else
                return Convert.ToDouble(resultStr, Util.Cultur);
        }

        /**
         *  @brief 실시간 전류 값
         *  @details 부하의 실시간 전류 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 부하가 출력 중인 전류 값
         */
        public double RealLoadCurr()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:CURRent?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            CommErrCount = 0;
            if (string.IsNullOrEmpty(resultStr))
                return double.NaN;
            else
                return Convert.ToDouble(resultStr, Util.Cultur);
        }
    }
}