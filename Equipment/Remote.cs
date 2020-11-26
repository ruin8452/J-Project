using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.Equipment
{
    public enum RemoteCommandList
    {
        DC_SWITCH1 = 0x3051,
        DC_SWITCH2,
        DC_SWITCH3,
        DC_SWITCH4,
    }

    /**
     *  @brief 리모트
     *  @details 리모트(IoT 모듈 대체 장비)에 대한 일련의 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Remote : Equipment
    {
        #region 싱글톤 패턴 구현

        private static Remote SingleTonObj = null;

        private Remote()
        {
            EquiName = "원격 통신";
        }

        public static Remote GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Remote();
            return SingleTonObj;
        }

        #endregion 싱글톤 패턴 구현

        struct CommandSendPacket
        {
            public byte SlaveId;
            public byte FuncCode;
            public ushort StartAddr;
            public ushort Data;
        }

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
        }

        /**
         *  @brief 리모트 명령어 전송
         *  @details 리모트에서 출력 ON,OFF 제어 명령을 하는 함수 
         *  
         *  @param RemoteCommandList cmd - 명령 코드(제어할 스위치 번호)
         *  @param ushort ctrlValue - 조작 값(ctrlValue : 1 - ON, 0 - OFF)
         *  
         *  @return bool - 명령에 대한 응답
         */
        public bool RemoteCommand(RemoteCommandList cmd, ushort ctrlValue)
        {
            CommandSendPacket sendPacket;

            sendPacket.SlaveId = (byte)(Rectifier.GetObj().CommId + 1);
            sendPacket.FuncCode = 0x06;
            sendPacket.StartAddr = Util.Swap((ushort)cmd);
            sendPacket.Data = Util.Swap(ctrlValue);

            byte[] sendStream = Util.StructureToByte(sendPacket);

            sendStream = (byte[])Util.MakeCrc16_byte(sendStream, sendStream.Length, Util.CrcReturnOption.ARRAY);

            // 송신
            TryResultFlag commResult = CommModule.CommSend(sendStream);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }

            // 수신
            commResult = CommModule.CommReceive(out byte[] reciveStream);
            if (commResult == TryResultFlag.FAIL) { CommErrCount++; return false; }

            CommErrCount = 0;
            return reciveStream.SequenceEqual(sendStream);
        }
    }
}
