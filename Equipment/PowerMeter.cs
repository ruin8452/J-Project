using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using J_Project.Manager;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace J_Project.Equipment
{
    /**
     *  @brief 파워미터
     *  @details 오실로스코프에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class PowerMeter : Equipment
    {
        private BackgroundWorker background = new BackgroundWorker();
        public double AcVolt { get; set; }

        public event EventHandler AcVoltRenewal;

        #region 싱글톤 패턴 구현
        private static PowerMeter _PowerMeter = null;

        private PowerMeter()
        {
            EquiName = "파워미터";

            background.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                e.Result = Math.Round(RealAcVolt(), 3);
            });
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                AcVolt = (double)e.Result;
                OnAcVoltRenewal(EventArgs.Empty);
            });

            EquiMonitoring.Interval = TimeSpan.FromMilliseconds(700);
            EquiMonitoring.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (background.IsBusy == false)
                    background.RunWorkerAsync();
            });
        }

        public static PowerMeter GetObj()
        {
            if (_PowerMeter == null) _PowerMeter = new PowerMeter();
            return _PowerMeter;
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
         *  @details 파워미터가 측정하고 있는 실시간 전압값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 파워미터가 측정 중인 전압 값
         */
        public double RealAcVolt()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:VOLTage:AC?");
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
                catch (Exception e)
                {
                    return double.NaN;
                }
            }
        }

        /**
         *  @brief 실시간 전류 값
         *  @details 파워미터가 측정하고 있는 실시간 전류값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 파워미터가 측정 중인 전류 값
         */
        public double RealAcCurr()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:CURRent:AC?");
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
         *  @brief 실시간 전력 값
         *  @details 파워미터가 측정하고 있는 실시간 전력값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 파워미터가 측정 중인 전력 값
         */
        public double RealPower()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:POWer:ACTive?");
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
         *  @brief 실시간 역률 값
         *  @details 파워미터가 측정하고 있는 실시간 역률값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 파워미터가 측정 중인 역률 값
         */
        public double RealPowerFactor()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:POWer:PFACtor?");
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
         *  @brief 실시간 소비전류 값
         *  @details 파워미터가 측정하고 있는 실시간 소비전류값을 요청한다
         *  
         *  @param 
         *  
         *  @return double - 현재 파워미터가 측정 중인 소비전류 값
         */
        public double RealCurrRms()
        {
            // 송신
            TryResultFlag commResult = CommModule.CommSend("MEASure:CURRent:RMS?");
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
         *  @brief AC전압값 갱신 이벤트 발생 함수
         *  @details AC전압값 갱신 이벤트 발생 시 실행되는 함수
         *  
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAcVoltRenewal(EventArgs e)
        {
            AcVoltRenewal?.Invoke(this, e);
        }
    }
}