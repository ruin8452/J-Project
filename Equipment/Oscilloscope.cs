using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;

namespace J_Project.Equipment
{
    /**
     *  @brief 오실로스코프
     *  @details 오실로스코프에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Oscilloscope : Equipment
    {
        #region 싱글톤 패턴 구현
        private static Oscilloscope _Osc = null;

        private Oscilloscope()
        {
            EquiName = "OSC";
            //CommModule = new CommModule_HalfDuplex();
            //CommModule.ConnectChange += new EventHandler(ConnectChagne);

            //EquiMonitoring.Interval = TimeSpan.FromMilliseconds(1000);
            //EquiMonitoring.Tick += RectMonitoring;
        }

        public static Oscilloscope GetObj()
        {
            if (_Osc == null) _Osc = new Oscilloscope();
            return _Osc;
        }
        #endregion

        /**
         *  @brief 커플링 모드
         *  @details 오실로스코프의 커플링 모드를 설정한다
         *  
         *  @param
         *  
         *  @return string - 현재 커플링 모드
         */
        public string CouplingMode()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CH1:COUPLING AC");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("CH1:COUPLING?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return resultStr;
        }

        /**
         *  @brief 측정 모드
         *  @details 오실로스코프의 측정 모드를 설정한다
         *  
         *  @param
         *  
         *  @return string - 현재 측정 모드
         */
        public string MeasurementMode()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASUrement:MEAS1:TYPe PK2pk");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("MEASUrement:MEAS1:TYPe?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return resultStr;
        }

        /**
         *  @brief TimeScale 
         *  @details 오실로스코프의 TimeScale를 설정한다
         *  
         *  @param
         *  
         *  @return double - 현재 TimeScale 값
         */
        public double TimeScale() // 단위 : s
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("HORizontal:MAIn:SCAle 0.025");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("HORizontal:MAIn:SCAle?");
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
         *  @brief RangeScale 
         *  @details 오실로스코프의 RangeScale 설정한다
         *  
         *  @param
         *  
         *  @return double - 현재 RangeScale 값
         */
        public double RangeScale() // 단위 : V
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CH1:SCAle 0.1");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("CH1:SCAle?");
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
         *  @brief 실시간 Pk2pk 측정
         *  @details 오실로스코프가 측정하고 있는 실시간 Pk2pk값을 요청한다
         *  
         *  @param
         *  
         *  @return double - 현재 오실로스코프가 측정중인 Pk2pk 값
         */
        public double RealPk2pk() // 단위 : V
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASUrement:MEAS1:VALue?");
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
         *  @brief 유저 설정 명령
         *  @details 유저가 원하는 
         *  
         *  @param string strCmd - 전송할 명령어
         *  
         *  @return string - 명령에 따른 응답값
         */
        public string UserCommand(string strCmd) // 단위 : V
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(strCmd);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return resultStr;
        }
    }
}
