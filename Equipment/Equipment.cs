using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using PropertyChanged;
using System.Diagnostics;
using System.Windows.Threading;

namespace J_Project.Equipment
{
    /**
     *  @brief 장비 관련 상위 클래스
     *  @details 장비의 기본적인 정보 및 기능을 담고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class Equipment
    {
        protected ICommModule CommModule = new NullComm();
        public bool IsConnected { get; set; }
        public string EquiCheckStr { get; set; }

        protected string EquiName;
        protected string EquiId;
        protected int CommSpeed;

        protected DispatcherTimer EquiMonitoring = new DispatcherTimer();

        private int commErrCount;
        protected int CommErrCount
        {
            get { return commErrCount; }
            set
            {
                commErrCount = value;
                Debug.WriteLine(EquiName + " " + commErrCount);
                if(commErrCount % 2 == 0)
                {
                    CommModule.TokenReset();
                }
                if (commErrCount >= 10)
                {
                    commErrCount = 0;
                    Disconnect();
                }
            }
        }

        /**
         *  @brief 접속
         *  @details 장비와 연결한다.
         *  
         *  @param string visaAddress - VISA ID
         *  
         *  @return string - 접속 결과
         */
        public virtual string Connect(string visaAddress)
        {
            CommModule = new UsbComm(visaAddress);
            EquiCheckStr = CommModule.Connect();

            return EquiCheckStr;
        }
        /**
         *  @brief 접속
         *  @details 장비와 연결한다.
         *  
         *  @param string portName - 포트명
         *  @param int baudRate - 통신 속도
         *  
         *  @return string - 접속 결과
         */
        public virtual string Connect(string portName, int baudRate)
        {
            CommModule = new SerialComm(portName, baudRate);
            EquiCheckStr = CommModule.Connect();

            return EquiCheckStr;
        }
        /**
         *  @brief 해제
         *  @details 장비와 연결 해제한다.
         *  
         *  @param
         *  
         *  @return TryResultFlag - 해제 결과
         */
        public virtual TryResultFlag Disconnect()
        {
            EquiCheckStr = string.Empty;
            IsConnected = false;

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
        public virtual string EquiCheck()
        {
            CommModule.CommSend("*IDN?");
            TryResultFlag commResult = CommModule.CommReceive(out string resultStr);
            EquiCheckStr = resultStr;

            if (commResult == TryResultFlag.SUCCESS)
            {
                IsConnected = true;
            }

            return EquiCheckStr;
        }

        /**
         *  @brief 모니터링 시작
         *  @details 장비의 측정 값을 주기적으로 요청하는 함수를 실행시키는 함수
         *  
         *  @param
         *  
         *  @return
         */
        public void MonitoringStart()
        {
            EquiMonitoring.Start();
        }
        /**
         *  @brief 모니터링 종료
         *  @details 장비의 측정 값을 주기적으로 요청하는 함수를 종료시키는 함수
         *  
         *  @param
         *  
         *  @return
         */
        public void MonitoringStop()
        {
            EquiMonitoring.Stop();
        }
    }
}