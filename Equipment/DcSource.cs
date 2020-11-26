using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;
using System.Diagnostics;

namespace J_Project.Equipment
{
    /**
     *  @brief DC 소스 장비
     *  @details DC 소스에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class DcSource : Equipment
    {
        #region 싱글톤 패턴 구현
        private static DcSource _DcSource = null;

        private DcSource()
        {
            EquiName = "DC 파워";
            //CommModule = new CommModule_HalfDuplex();
            //CommModule.ConnectChange += new EventHandler(ConnectChagne);

            //EquiMonitoring.Interval = TimeSpan.FromMilliseconds(1000);
            //EquiMonitoring.Tick += RectMonitoring;
        }

        public static DcSource GetObj()
        {
            if (_DcSource == null) _DcSource = new DcSource();
            return _DcSource;
        }
        #endregion

        /**
         *  @brief DC 전압 설정
         *  @details DC 소스 장비의 출력 DC 전압값을 설정한다.
         *  
         *  @param double dcv - 설정할 DC 전압값
         *  
         *  @return double - 현재 DC 소스에 설정되어 있는 DC 전압 값
         */
        public double DcVoltSet(double dcv)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("VOLTage " + dcv);
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
         *  @brief DC 전류 제한 설정
         *  @details DC 소스 장비의 출력 DC 전류 제한값을 설정한다.
         *  
         *  @param double dcc - 설정할 DC 전류값
         *  
         *  @return double - 현재 DC 소스에 설정되어 있는 DC 전류 값
         */
        public double DcCurrSet(double dcc)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CURRent " + dcc);
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
         *  @brief AC 출력 설정
         *  @details AC 소스 장비의 출력을 설정한다.
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
        public CtrlFlag? DcPowerCtrl(CtrlFlag flag)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("OUTPut " + flag);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("OUTPut?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return (CtrlFlag?)Convert.ToInt32(resultStr, Util.Cultur);
        }

        /**
         *  @brief 실시간 DC 출력 전압 값
         *  @details DC 소스 장비의 실시간 DC 출력 전압 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 DC 소스가 출력 중인 전압 값
         */
        public double RealOutputVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEAS:VOLT?");
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
         *  @brief 실시간 DC 출력 전류 값
         *  @details DC 소스 장비의 실시간 DC 출력 전류 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 DC 소스가 출력 중인 전류 값
         */
        public double RealOutputCurr()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEAS:CURR?");
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
