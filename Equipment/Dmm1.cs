using J_Project.Communication.CommFlags;
using J_Project.Data;
using J_Project.Manager;
using System;
using System.ComponentModel;

namespace J_Project.Equipment
{
    /**
     *  @brief 전압 측정용 DMM 장비
     *  @details 전압 측정용 DMM에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Dmm1 : Equipment
    {
        private BackgroundWorker background = new BackgroundWorker();
        public double DcVolt { get; set; }

        #region 싱글톤 패턴 구현
        private static Dmm1 _Dmm1 = null;

        private Dmm1()
        {
            EquiName = "DMM1";
            EquiId = EquiConnectID.GetObj().Dmm1ID;

            background.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                double result = Math.Round(RealDcVolt(), 3);
                e.Result = result;
            });
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                DcVolt = (double)e.Result;
            });

            EquiMonitoring.Interval = TimeSpan.FromMilliseconds(1000);
            EquiMonitoring.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (background.IsBusy == false)
                    background.RunWorkerAsync();
            });
        }

        public static Dmm1 GetObj()
        {
            if (_Dmm1 == null) _Dmm1 = new Dmm1();
            return _Dmm1;
        }
        #endregion

        /**
         *  @brief 접속 해제
         *  @details DMM 장비와의 연결을 해제한다.
         *  
         *  @param
         *  
         *  @return TryResultFlag - 접속 해제 여부
         */
        public override TryResultFlag Disconnect()
        {
            EquiCheckStr = string.Empty;
            IsConnected = false;
            EquiMonitoring.Stop();

            return CommModule.Disconnect();
        }
        /**
         *  @brief 장비 체크
         *  @details 해당 장비의 고유 번호를 요청한다.
         *  
         *  @param
         *  
         *  @return string - 장비의 고유 번호
         */
        public override string EquiCheck()
        {
            CommModule.CommSend("*IDN?");
            TryResultFlag result = CommModule.CommReceive(out string equiStr);
            EquiCheckStr = equiStr;

            if (result == TryResultFlag.SUCCESS)
            {
                IsConnected = true;
                MonitoringStart();
            }

            return EquiCheckStr;
        }

        /**
         *  @brief 실시간 전압 값
         *  @details DMM이 측정하고 있는 실시간 전압값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 DMM이 측정 중인 전압 값
         */
        public double RealDcVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:VOLTage:DC? 100");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return double.NaN; }

            CommErrCount = 0;
            if (string.IsNullOrEmpty(resultStr))
                return double.NaN;
            else
            {
                try
                {
                    return Convert.ToDouble(resultStr, Util.Cultur);
                }
                catch(Exception e)
                {
                    return double.NaN;
                }
            }
        }

        /**
         *  @brief 실시간 전류 값
         *  @details DMM이 측정하고 있는 실시간 전류값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 DMM이 측정 중인 전류 값
         */
        public double RealDcCurr()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:Current:DC?");
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
         *  @brief 측정 범위 설정
         *  @details DMM의 측정 범위를 설정한다
         *  
         *  @param 
         *  
         *  @return int? - 현재 측정 범위 값
         */
        public int? AutoRangeSet()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CONFigure:AUTO 1");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("CONFigure:AUTO?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return Convert.ToInt32(resultStr, Util.Cultur);
        }

        /**
         *  @brief 측정 전압 타입 설정
         *  @details DMM의 측정 전압 타입을 설정한다 (AC, DC)
         *  
         *  @param 
         *  
         *  @return string - 현재 측정 전압 타입 값
         */
        public string DisplayVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("CONFigure:VOLTage:DC");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }
            CommModule.TokenReset();

            commResult = CommModule.CommSend("CONFigure:FUNCtion?");
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            // 수신
            commResult = CommModule.CommReceive(out string resultStr);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return null; }

            CommErrCount = 0;
            return resultStr;
        }
    }
}
