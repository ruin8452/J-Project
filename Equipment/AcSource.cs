using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;
using System.Diagnostics;

namespace J_Project.Equipment
{
    /**
     *  @brief AC 소스 장비
     *  @details AC 소스에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class AcSource : Equipment
    {
        #region 싱글톤 패턴 구현

        private static AcSource _AcSource = null;

        /**
         *  @brief 생성자
         *  @details
         *  
         *  @param 
         *  
         *  @return
         */
        private AcSource()
        {
            EquiName = "AC 파워";
        }

        public static AcSource GetObj()
        {
            if (_AcSource == null) _AcSource = new AcSource();
            return _AcSource;
        }

        #endregion 싱글톤 패턴 구현

        /**
         *  @brief AC 전압 설정
         *  @details AC 소스 장비의 출력 AC 전압값을 설정한다.
         *  
         *  @param double acv - 설정할 AC 전압값
         *  
         *  @return double - 현재 AC 소스에 설정되어 있는 AC 전압 값
         */
        public double AcVoltSet(double acv)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":VOLTage " + acv);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend(":VOLTage?");
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
         *  @brief AC 전류 제한 설정
         *  @details AC 소스 장비의 출력 AC 전류 제한값을 설정한다.
         *  
         *  @param double acc - 설정할 AC 전류값
         *  
         *  @return double - 현재 AC 소스에 설정되어 있는 AC 전류 값
         */
        public double AcCurrSet(double acc)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":CURRent " + acc);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend(":CURRent?");
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
         *  @brief AC 주파수 설정
         *  @details AC 소스 장비의 출력 AC 주파수를 설정한다.
         *  
         *  @param double freq - 설정할 주파수값
         *  
         *  @return double - 현재 AC 소스에 설정되어 있는 주파수 값
         */
        public double AcFreqSet(double freq)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":FREQuency " + freq);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend(":FREQuency?");
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
        public CtrlFlag? AcPowerCtrl(CtrlFlag flag)
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":OUTPut:GENeral " + flag);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend(":OUTPut?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            try
            {
                CtrlFlag? result = (CtrlFlag?)Convert.ToInt32(resultStr, Util.Cultur);
                CommErrCount = 0;
                return result;
            }
            catch(Exception e)
            {
                Debug.WriteLine("ErrPos08 : " + e.Message);
                CommErrCount++;
                return null;
            }
        }

        /**
         *  @brief 실시간 AC 출력 전압 값
         *  @details AC 소스 장비의 실시간 AC 출력 전압 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 AC 소스가 출력 중인 전압 값
         */
        public double RealOutputVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":MEASure:VOLTage?");
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
         *  @brief 실시간 AC 출력 전류 값
         *  @details AC 소스 장비의 실시간 AC 출력 전류 값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 AC 소스가 출력 중인 전류 값
         */
        public double RealOutputCurr()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":MEASure:CURRent?");
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
         *  @brief 실시간 주파수 값
         *  @details AC 소스 장비의 실시간 주파수를 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 AC 소스가 출력 중인 주파수 값
         */
        public double RealOutputFreq()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend(":MEASure:FREQuency?");
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