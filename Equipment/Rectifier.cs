using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using J_Project.Manager;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace J_Project.Equipment
{
    public enum CommandList
    {
        ERROR_MESSAGE_READ = 0x1100,
        CAL_V_APPLY,
        CAL_I_APPLY,
        HW_FAIL1,
        HW_FAIL2,
        HW_FAIL3,
        HW_FAIL4,
        UNDER_VOLT_ALARM,
        CAL_RESET,
        I_REF_SET,
        V_REF_SET = 0x1110,
        AC_IN_CAL,
        ADC_V_CAL,
        ADC_I_CAL,
        RTC_SET = 0x1116,
        EEPROM_WRITE = 0x1119,
        EEPROM_READ = 0x1120,
        SYSTEM_POWER_ON,
        SYSTEM_POWER_OFF,
        DAC_V_CAL,
        DAC_I_CAL,
        DC_48V_TEST,
        SW_RESET,
        BATTERY_TEST
    }

    /**
     *  @brief 정류기
     *  @details 정류기에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class Rectifier : Equipment
    {
        private BackgroundWorker background = new BackgroundWorker();

        public event EventHandler MonitorRenewal;

        public int    Reserved1           { get; set; }
        public int    Reserved2           { get; set; }
        public double AcInputVolt         { get; set; } // AC 입력 전압
        public double DcOutputVolt        { get; set; } // DC 출력 전압
        public double DcOutputCurr        { get; set; } // DC 출력 전류
        public ObservableCollection<double> DcSwOutVolt       { get; set; }//DC 스위치 출력 전압(1 ~ 4)
        public short  SystemTemp          { get; set; } // 시스템 온도
        // 알람4
        public bool   Flag_RectFail       { get; set; } // 정류기 오류 플래그
        public bool   Flag_AcFail         { get; set; } // AC 오류 플래그
        public bool   Flag_BatFail        { get; set; } // 배터리 오류 플래그
        public bool   Flag_BatEx          { get; set; } // 배터리 교체 플래그
        public string AcInVoltMode        { get; set; } // AC 모드
        public bool   Flag_UnberDcOutVolt { get; set; } // DC 출력 저전압 오류 플래그
        public bool   Flag_DcOverLoad     { get; set; } // 과부하 오류 플래그
        public bool   Flag_AcRelayOnOff   { get; set; } // AC 릴레이 플래그
        public bool   Flag_SystemCutOff   { get; set; } // 시스템 다운 플래그
        public byte   DcOutVoltFailNum    { get; set; } // DC 출력전압 오류 번호
        public byte   DcOutCurrFailNum    { get; set; } // DC 출력전류 오류 번호
        // 알람5
        public bool   Flag_OverDcOutVolt  { get; set; } // DC 출력 고전압 오류 플래그
        public bool   Flag_OverDcOutCurr  { get; set; } // DC 출력 고전류 오류 플래그
        public bool   Flag_OverAcInVolt   { get; set; } // AC 고전압 오류 플래그
        public bool   Flag_UnderAcInVolt  { get; set; } // AC 저전압 오류 플래그
        public bool   Flag_TempHeatFail   { get; set; } // 시스템 온도 과열 플래그
        public bool[] DcSwLed3V           { get; set; } // Dc 스위치 3V Led
        public bool[] DcSwLed40V          { get; set; } // Dc 스위치 40V Led

        public int    CommandCheck        { get; set; } //
        public int    EEPRomData          { get; set; } //
        // 스위치 모듈
        public ObservableCollection<bool> SwLed               { get; set; } // 스위치 Led, true:Off, false:On
        public byte   CommId              { get; set; } // 통신 ID
        public bool   LocalRemoteLed      { get; set; } // 로컬/리모트 Led, true:Local, false:Remote

        public int    Reserved3           { get; set; }
        // 배터리 통신연결
        public ObservableCollection<bool> BatteryComm         { get; set; } // 배터리 연결상태, true:연결, false:단절

        public DateTime RectTime          { get; set; } // 정류기 RTC 시간
        public int    Reserved4           { get; set; }
        public int    Reserved5           { get; set; }
        public double FwVersion           { get; set; } // 펌웨어 버전

        #region 싱글톤 패턴 구현

        private static Rectifier SingleTonObj = null;
        int sendCount = 0;
        int receiveCount = 0;

        private Rectifier()
        {
            EquiName = "정류기";

            DcSwOutVolt = new ObservableCollection<double>();
            DcSwLed3V = new bool[4];
            DcSwLed40V = new bool[4];
            SwLed = new ObservableCollection<bool>();
            BatteryComm = new ObservableCollection<bool>();

            for (int i = 0; i < 4; i++)
            {
                DcSwOutVolt.Add(0);
                SwLed.Add(false);
            }
            for (int i = 0; i < 8; i++)
            {
                BatteryComm.Add(false);
            }

            background.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {   
                RectMonitoring();
            });

            EquiMonitoring.Interval = TimeSpan.FromMilliseconds(1000);
            EquiMonitoring.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (background.IsBusy == false)
                    background.RunWorkerAsync();
            });
        }

        public static Rectifier GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Rectifier();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        #region ModBus Packet 관련 소스

        // Monitoring 데이터 요청
        // 시작 주소 : 0x1000
        // 길이 : 15(word)
        // ModBus RTU 데이터 요청 패킷(CRC 부분 제외)
        struct MonitorSendPacket
        {
            public byte SlaveId;
            public byte FuncCode;
            public ushort StartAddr;
            public ushort DataLength;
        }

        struct CommandSendPacket
        {
            public byte SlaveId;
            public byte FuncCode;
            public ushort StartAddr;
            public ushort RegNum;
            public byte ByteCount;
            public ushort DataCode;
            public ushort Data1;
            public ushort Data2;
            public ushort Data3;
        }

        const byte SLAVE_ID = 0x73;
        const ushort MONITOR_ADDR = 0x1000;
        const byte READ_INPUT_REGISTERS = 0x04;
        const byte WRITE_MULTIPLE_REGISTERS = 0x10;

        #endregion

        /**
         *  @brief 접속
         *  @details 장비와 연결한다.
         *  
         *  @param string portName - 포트 이름
         *  @param string baudRate - 통신 속도
         *  
         *  @return string - 접속 결과
         */
        public override string Connect(string portName, int baudRate)
        {
            sendCount = 0;
            receiveCount = 0;

            //queueComm = new QueueComm(portName, baudRate);
            //EquiCheckStr = queueComm.Connect();

            CommModule = new SerialComm(portName, baudRate);
            EquiCheckStr = CommModule.Connect();
            if (EquiCheckStr == "Connected!")
            {
                IsConnected = true;
                EquiMonitoring.Start();
            }
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
        public override TryResultFlag Disconnect()
        {
            EquiCheckStr = string.Empty;
            IsConnected = false;
            EquiMonitoring.Stop();
            return CommModule.Disconnect();
            //return queueComm.Disconnect();
        }

        /**
         *  @brief 정류기 모니터링
         *  @details 정류기에게 모니터링 데이터를 요청한다
         *  
         *  @param
         *  
         *  @return bool - 요청 결과
         */
        public bool RectMonitoring()
        {
            // 정류기 모니터링 패킷 구성
            MonitorSendPacket sendPacket;
            sendPacket.SlaveId = SLAVE_ID;
            sendPacket.FuncCode = READ_INPUT_REGISTERS;
            sendPacket.StartAddr = Util.Swap(MONITOR_ADDR);
            sendPacket.DataLength = Util.Swap(23);

            byte[] sendStream = Util.StructureToByte(sendPacket);
            sendStream = (byte[])Util.MakeCrc16_byte(sendStream, sendStream.Length, Util.CrcReturnOption.ARRAY);

            // 송신
            TryResultFlag commResult = CommModule.CommSend(sendStream);
            //TryResultFlag commResult = queueComm.CommSend(sendStream, out int code);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }
            sendCount++;
            // 수신
            commResult = CommModule.CommReceive(out byte[] reciveStream);
            //commResult = queueComm.CommReceive(out byte[] reciveStream, code);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }

            reciveStream = ReciveDataCheck(reciveStream);
            if(reciveStream == null) { CommErrCount++; return false; }
            receiveCount++;

            Debug.WriteLine("send : " + sendCount + " receive : " + receiveCount);

            MatchMonitoringData(reciveStream);

            CommErrCount = 0;
            return true;
        }

        /**
         *  @brief 정류기 명령 전송
         *  @details 정류기에게 명령을 전송한다
         *  
         *  @param CommandList cmd - 명령 코드
         *  @param ushort inputData1 - 데이터 1
         *  @param ushort inputData2 - 데이터 2
         *  @param ushort inputData3 - 데이터 3
         *  
         *  @return bool - 명령 전송 결과
         */
        public bool RectCommand(CommandList cmd, ushort inputData1 = 0, ushort inputData2 = 0, ushort inputData3 = 0)
        {
            CommandSendPacket sendPacket;

            sendPacket.SlaveId = SLAVE_ID;
            sendPacket.FuncCode = WRITE_MULTIPLE_REGISTERS;
            sendPacket.StartAddr = Util.Swap((ushort)cmd);
            sendPacket.RegNum = Util.Swap(4);
            sendPacket.ByteCount = 8;
            sendPacket.Data1 = Util.Swap(inputData1);
            sendPacket.Data2 = Util.Swap(inputData2);
            sendPacket.Data3 = Util.Swap(inputData3);
            
            switch(cmd)
            {
                case CommandList.CAL_V_APPLY:
                    sendPacket.DataCode = Util.Swap(1);
                    break;
                case CommandList.CAL_I_APPLY :
                    sendPacket.DataCode = Util.Swap(2);
                    break;
                case CommandList.HW_FAIL1 :
                    sendPacket.DataCode = Util.Swap(3);
                    break;
                case CommandList.HW_FAIL2 :
                    sendPacket.DataCode = Util.Swap(4);
                    break;
                case CommandList.HW_FAIL3 :
                    sendPacket.DataCode = Util.Swap(5);
                    break;
                case CommandList.HW_FAIL4 :
                    sendPacket.DataCode = Util.Swap(6);
                    break;
                case CommandList.UNDER_VOLT_ALARM :
                    sendPacket.DataCode = Util.Swap(7);
                    break;
                case CommandList.CAL_RESET :
                    sendPacket.DataCode = Util.Swap(8);
                    break;
                case CommandList.I_REF_SET :
                    sendPacket.DataCode = Util.Swap(9);
                    break;
                case CommandList.V_REF_SET :
                    sendPacket.DataCode = Util.Swap(10);
                    break;
                case CommandList.AC_IN_CAL :
                    sendPacket.DataCode = Util.Swap(11);
                    break;
                case CommandList.ADC_V_CAL :
                    sendPacket.DataCode = Util.Swap(12);
                    break;
                case CommandList.ADC_I_CAL :
                    sendPacket.DataCode = Util.Swap(13);
                    break;
                case CommandList.RTC_SET :
                    sendPacket.DataCode = Util.Swap(16);
                    break;
                case CommandList.EEPROM_WRITE :
                    sendPacket.DataCode = Util.Swap(19);
                    break;
                case CommandList.EEPROM_READ :
                    sendPacket.DataCode = Util.Swap(20);
                    break;
                case CommandList.SYSTEM_POWER_ON :
                    sendPacket.DataCode = Util.Swap(21);
                    break;
                case CommandList.SYSTEM_POWER_OFF :
                    sendPacket.DataCode = Util.Swap(22);
                    break;
                case CommandList.DAC_V_CAL :
                    sendPacket.DataCode = Util.Swap(23);
                    break;
                case CommandList.DAC_I_CAL :
                    sendPacket.DataCode = Util.Swap(24);
                    break;
                case CommandList.DC_48V_TEST :
                    sendPacket.DataCode = Util.Swap(25);
                    break;
                case CommandList.SW_RESET :
                    sendPacket.DataCode = Util.Swap(26);
                    break;
                case CommandList.BATTERY_TEST:
                    sendPacket.DataCode = Util.Swap(27);
                    break;
                default:
                    sendPacket.DataCode = 0;
                    break;
            }

            byte[] sendStream = Util.StructureToByte(sendPacket);

            List<byte> listStream = new List<byte>(sendStream);

            // 바이트 배열을 구조체로 변경시 구조체의 최대 크기의 자료형으로 사이즈가 맞춰진다.
            // 이 프로토콜에서 최대 사이즈는 ushort이므로 2byte로 강제 맞춰진다.
            // 2byte로 짝이 맞아 떨어지지 않는 자료형이 중간에 낄 경우 2byte로 강제로 늘려진다.
            // 따라서 이 프로토콜에서 짝이 맞지 않는 ByteCount는 한 바이트가 쓰레기 값으로 들어간다.
            // 그것을 제거하기 위해 아래의 코드가 들어간다.
            listStream.RemoveAt(7);
            listStream = (List<byte>)Util.MakeCrc16_byte(listStream.ToArray(), listStream.Count, Util.CrcReturnOption.LIST);

            // 송신
            TryResultFlag commResult = CommModule.CommSend(listStream.ToArray());
            //TryResultFlag commResult = queueComm.CommSend(listStream.ToArray(), out int code);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }

            // 수신
            commResult = CommModule.CommReceive(out byte[] reciveStream);
            //commResult = queueComm.CommReceive(out byte[] reciveStream, code);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }

            CommErrCount = 0;
            return ReciveDataCheck(listStream, new List<byte>(reciveStream));
        }

        /**
         *  @brief 커맨드 명령에 대한 수신 데이터 검사
         *  @details 정류기로 커맨드 명령 보냈을 때, 수신 받은 데이터가 정상적인지 체크
         *  
         *  @param List<byte> sendCommand - 명령 데이터
         *  @param List<byte> reciveDate - 수신받은 데이터
         *  
         *  @return bool - 명령 전송 결과
         */
        private bool ReciveDataCheck(List<byte> sendCommand, List<byte> reciveDate)
        {
            // 데이터 수신 확인
            if (reciveDate.Count != 8)
                return false;

#warning 정류기 커맨드 수신 CRC 임시 제거
            // CRC 체크
            //byte[] crcByte = (byte[])Util.MakeCrc16_byte(reciveDate.ToArray(), reciveDate.Count - 2, Util.CrcReturnOption.CRC_ONLY);
            //if (crcByte[0] != reciveDate[reciveDate.Count - 2] || crcByte[1] != reciveDate[reciveDate.Count - 1])
            //    return false;

            reciveDate.RemoveRange(reciveDate.Count - 2, 2);  // CRC 제거
            sendCommand = sendCommand.GetRange(0, reciveDate.Count);

            return reciveDate.SequenceEqual(sendCommand);
        }

        /**
         *  @brief 모니터링 데이터 요청에 대한 수신 데이터 검사
         *  @details 정류기로 모니터링 데이터 요청 명령을 보냈을 때, 수신 받은 데이터가 정상적인지 체크
         *  
         *  @param byte[] reciveDate - 수신받은 데이터
         *  
         *  @return byte[] - 헤더를 제거한 데이터 원본
         */
        private byte[] ReciveDataCheck(byte[] reciveStream)
        {
            List<byte> tempStream = new List<byte>(reciveStream);

            // 올바른 데이터 배열 수신 확인
            if (tempStream.Count != 51)
                return null;

            // CRC 체크
            byte[] crcByte = (byte[])Util.MakeCrc16_byte(tempStream.ToArray(), tempStream.Count - 2, Util.CrcReturnOption.CRC_ONLY);
            if (crcByte[0] != tempStream[tempStream.Count - 2] || crcByte[1] != tempStream[tempStream.Count - 1])
                return null;

            // SlaveID 체크
            if (tempStream[0] != SLAVE_ID)
                return null;

            // 길이 체크(ID, FC, length, CRC 항목 제외한 나머지)
            if (tempStream.Count - 5 != tempStream[2])
                return null;

            // 헤더 및 CRC 제거
            tempStream.RemoveRange(0, 3);   // 헤더 제거(ID, FC, Length)
            tempStream.RemoveRange(tempStream.Count-2, 2);  // CRC 제거
            
            return tempStream.ToArray();
        }

        /**
         *  @brief 수신 받은 모니터링 데이터 매핑
         *  @details 수신 받은 모니터링 데이터를 변수에 매핑하는 함수
         *  
         *  @param byte[] reciveStream - 수신받은 데이터 원본
         *  
         *  @return
         */
        private void MatchMonitoringData(byte[] reciveStream)
        {
            if (reciveStream == null)
                return;

            Reserved1 = (reciveStream[1] << 8) + reciveStream[0];
            Reserved2 = (reciveStream[3] << 8) + reciveStream[2];
            AcInputVolt = (short)((reciveStream[5] << 8) + reciveStream[4]) / 100.0;
            DcOutputVolt = (short)((reciveStream[7] << 8) + reciveStream[6]) / 100.0;
            DcOutputCurr = (short)((reciveStream[9] << 8) + reciveStream[8]) / 100.0;
            DcSwOutVolt[0] = (short)((reciveStream[11] << 8) + reciveStream[10]) / 100.0;
            DcSwOutVolt[1] = (short)((reciveStream[13] << 8) + reciveStream[12]) / 100.0;
            SystemTemp = (short)((reciveStream[15] << 8) + reciveStream[14]);
            DcSwOutVolt[2] = (short)((reciveStream[17] << 8) + reciveStream[16]) / 100.0;
            DcSwOutVolt[3] = (short)((reciveStream[19] << 8) + reciveStream[18]) / 100.0;

            Flag_RectFail = (reciveStream[20] & 0x01) != 0 ? true : false;
            Flag_AcFail = (reciveStream[20] & 0x02) != 0 ? true : false;
            Flag_BatFail = (reciveStream[20] & 0x04) != 0 ? true : false;
            Flag_BatEx = (reciveStream[20] & 0x08) != 0 ? true : false;
            AcInVoltMode = (reciveStream[20] & 0x10) != 0 ? "200V" : "100V";
            Flag_UnberDcOutVolt = (reciveStream[20] & 0x20) != 0 ? true : false;
            Flag_DcOverLoad = (reciveStream[20] & 0x40) != 0 ? true : false;
            Flag_AcRelayOnOff = (reciveStream[20] & 0x80) != 0 ? false : true;  // false : 정상, true : 정전
            Flag_SystemCutOff = (reciveStream[21] & 0x01) != 0 ? true : false;
            DcOutVoltFailNum = (byte)(reciveStream[21] >> 1 & 0x07);
            DcOutCurrFailNum = (byte)(reciveStream[21] >> 4 & 0x07);

            Flag_OverDcOutVolt = (reciveStream[22] & 0x01) != 0 ? true : false;
            Flag_OverDcOutCurr = (reciveStream[22] & 0x02) != 0 ? true : false;
            Flag_OverAcInVolt  = (reciveStream[22] & 0x04) != 0 ? true : false;
            Flag_UnderAcInVolt = (reciveStream[22] & 0x08) != 0 ? true : false;
            Flag_TempHeatFail  = (reciveStream[22] & 0x10) != 0 ? true : false;
            DcSwLed3V[0]  = (reciveStream[23] & 0x01) != 0 ? true : false;
            DcSwLed3V[1]  = (reciveStream[23] & 0x02) != 0 ? true : false;
            DcSwLed3V[2]  = (reciveStream[23] & 0x04) != 0 ? true : false;
            DcSwLed3V[3]  = (reciveStream[23] & 0x08) != 0 ? true : false;
            DcSwLed40V[0] = (reciveStream[23] & 0x10) != 0 ? true : false;
            DcSwLed40V[1] = (reciveStream[23] & 0x20) != 0 ? true : false;
            DcSwLed40V[2] = (reciveStream[23] & 0x40) != 0 ? true : false;
            DcSwLed40V[3] = (reciveStream[23] & 0x80) != 0 ? true : false;

            CommandCheck = (reciveStream[25] << 8) + reciveStream[24];
            EEPRomData = (reciveStream[27] << 8) + reciveStream[26];

            SwLed[0] = (reciveStream[28] & 0x01) != 0 ? false : true;
            SwLed[1] = (reciveStream[28] & 0x02) != 0 ? false : true;
            SwLed[2] = (reciveStream[28] & 0x04) != 0 ? false : true;
            SwLed[3] = (reciveStream[28] & 0x08) != 0 ? false : true;
            CommId = (byte)((reciveStream[28] >> 4) & 0x03);
            LocalRemoteLed = (reciveStream[28] & 0x40) != 0 ? true : false;

            Reserved3 = (reciveStream[31] << 8) + reciveStream[30];

            BatteryComm[0] = (reciveStream[32] & 0x01) != 0 ? true : false;
            BatteryComm[1] = (reciveStream[32] & 0x02) != 0 ? true : false;
            BatteryComm[2] = (reciveStream[32] & 0x04) != 0 ? true : false;
            BatteryComm[3] = (reciveStream[32] & 0x08) != 0 ? true : false;
            BatteryComm[4] = (reciveStream[32] & 0x10) != 0 ? true : false;
            BatteryComm[5] = (reciveStream[32] & 0x20) != 0 ? true : false;
            BatteryComm[6] = (reciveStream[32] & 0x40) != 0 ? true : false;
            BatteryComm[7] = (reciveStream[32] & 0x80) != 0 ? true : false;

            RectTime = new DateTime(reciveStream[35] + 2000,
                                    reciveStream[34] != 0 ? reciveStream[34] : 1,    // 월 값이 0이면 오류
                                    reciveStream[37] != 0 ? reciveStream[37] : 1,    // 일 값이 0이면 오류
                                    reciveStream[36],
                                    reciveStream[39],
                                    reciveStream[38]);
            Reserved4 = (reciveStream[40] << 8) + reciveStream[41];
            Reserved5 = (reciveStream[42] << 8) + reciveStream[43];
            FwVersion = reciveStream[44] + (reciveStream[45] * 0.1);

            // 수신 받은 입력 AC값이 60 이하면 0으로 모니터링
            AcInputVolt = AcInputVolt > 60 ? AcInputVolt : 0;

            OnMonitorRenewal(EventArgs.Empty);
        }

        /**
         *  @brief 모니터링 값 갱신 이벤트 발생 함수
         *  @details 정류기 모니터링 갱신 이벤트 발생 시 실행되는 함수
         *  
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnMonitorRenewal(EventArgs e)
        {
            MonitorRenewal?.Invoke(this, e);
        }
    }
}