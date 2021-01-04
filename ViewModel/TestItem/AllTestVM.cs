using J_Project.Communication.CommFlags;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.UI.SubWindow;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace J_Project.ViewModel.TestItem
{
    public enum FirstTestOrder
    {
        IsolRes = 1,   // 절연저항
        IsolPress,     // 절연내압
        PowerSupply,   // 전원 공급
        Inrush,        // 돌입전류
        IdSet,         // ID 세팅
        Temp,          // 온도센서 점검
        Leakage,       // 누설전류
        LocalSwitch,   // 로컬 스위치 확인
        Remote,        // 리모트 통신 확인
        Battery,       // 배터리 통신 확인
        LedCheck,      // LED 검사
        Regul200V_0,   // 레귤레이션 200V 0
        Regul200V_1,   // 레귤레이션 200V 1
        Regul200V_2,   // 레귤레이션 200V 2
        Regul200V_3,   // 레귤레이션 200V 3
        Regul200V_4,   // 레귤레이션 200V 4
        Regul200V_5,   // 레귤레이션 200V 5
        Regul200V_6,   // 레귤레이션 200V 6
        Regul200V_7,   // 레귤레이션 200V 7
        Regul200V_8,   // 레귤레이션 200V 8
        Noise,         // 리플 노이즈
        PowerFacter,   // 역률
        Efficiency,    // 효율
        OutputLow,     // 출력 저전압
        OutputHigh,    // 출력 고전압
        AcLow0,        // AC 저전압 200V
        AcLow1,        // AC 저전압 100V
        AcHigh,        // AC 고전압
        OverLoad0,     // 출력 과부하 200V
        OverLoad1,     // 출력 과부하 100V
        Regul100V_0,   // 레귤레이션 100V 0
        Regul100V_1,   // 레귤레이션 100V 1
        Regul100V_2,   // 레귤레이션 100V 2
        Regul100V_3,   // 레귤레이션 100V 3
        Regul100V_4,   // 레귤레이션 100V 4
        Regul100V_5,   // 레귤레이션 100V 5
        Regul100V_6,   // 레귤레이션 100V 6
        Regul100V_7,   // 레귤레이션 100V 7
        Regul100V_8,   // 레귤레이션 100V 8
        AcBlackOut,    // AC 정전
        Rtc            // RTC
    }

    public enum SecondTestOrder
    {
        NoLoad = 1,    // 무부하
        OutputCheck,   // 출력 전압 체크
        Led,           // Led 체크
        Remote,        // 리모트 통신 확인
        Battery,       // 배터리 통신 확인
        LocalSwitch,   // 로컬 스위치 확인
        Connecter,     // 출력 커넥터 체크
        PowerFacter,   // 역률
        Noise,         // 리플 노이즈
        Rtc            // RTC

    }

    [ImplementPropertyChanged]
    public abstract class AllTestVM
    {
        protected bool subWinResult;

        protected const int MAX_CAL_TRY_COUNT = 5;
        protected const int MAX_TRY_COUNT = 5;
        protected const int AC_ERR_RANGE = 5;
        protected const int LOAD_ERR_RANGE = 3;

        protected int TestOrterNum;
        public int TotalStepNum;
        public static string ReportSavePath;

        // 첫 줄 기본 정보 삽입을 위해 배열의 크기 1+
        private static int FirstOrderCnt = Enum.GetNames(typeof(FirstTestOrder)).Length + 1;
        private static int SecondOrderCnt = Enum.GetNames(typeof(FirstTestOrder)).Length + 1;
        public static string[][] FirstOrder = new string[FirstOrderCnt][];
        public static string[][] SecondOrder = new string[SecondOrderCnt][];

        protected (string, string) resultData = ("판단 불가", "NG(불합격)");
        public StringBuilder TestLog;

        /**
         *  @brief 양산 테스트 데이터 배열 초기화
         *  @details 양산 테스트 데이터가 담긴 배열을 초기화한다
         *  
         *  @param 
         *  
         *  @return 
         */
        public static void FirstOrderInit()
        {
            // 맨 앞쪽 데이터 4개(기본정보, 절연저항, 절연내압, 전원 공급)는 패스
            for(int i = 4; i < FirstOrderCnt; i++)
            {
                FirstOrder[i][2] = "판단 불가";
                FirstOrder[i][3] = "NG(불합격)";
            }
        }
        /**
         *  @brief 출하 테스트 데이터 배열 초기화
         *  @details 출하 테스트 데이터가 담긴 배열을 초기화한다
         *  
         *  @param 
         *  
         *  @return 
         */
        public static void SecondOrderInit()
        {
            for (int i = 1; i < SecondOrderCnt; i++)
            {
                SecondOrder[i][2] = "판단 불가";
                SecondOrder[i][3] = "NG(불합격)";
            }
        }

        /**
         *  @brief AC 설정
         *  @details AC 소스의 전압, 전류, 주파수를 설정한다
         *  
         *  @param double acv - AC 전압
         *  @param double acc - AC 전류
         *  @param double freq - 주파수
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag AcSourceSet(double acv, double acc, double freq)
        {
            
            AcSource acSource = AcSource.GetObj();
            double? result = null;

            // 전압 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                result = acSource.AcVoltSet(acv);
                if (result == acv) break;
            }
            if (result != acv) return StateFlag.AC_VOLT_SET_ERR;

            // 전류 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                result = acSource.AcCurrSet(acc);
                if (result == acc) break;
            }
            if (result != acc) return StateFlag.AC_CURR_SET_ERR;

            // 주파수 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                result = acSource.AcFreqSet(freq);
                if (result == freq) break;
            }
            if (result != freq) return StateFlag.AC_FREQ_SET_ERR;

            return StateFlag.PASS;
        }
        /**
         *  @brief AC 설정
         *  @details AC 소스의 전압을 설정한다
         *  
         *  @param double acv - AC 전압
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag AcSourceSet(double acv)
        {
            AcSource acSource = AcSource.GetObj();

            // 전압 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                double? result = acSource.AcVoltSet(acv);

                if (result == acv)
                    return StateFlag.PASS;
            }
            return StateFlag.AC_VOLT_SET_ERR;
        }

        /**
         *  @brief AC 출력 ON
         *  @details AC 소스의 출력을 ON한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag AcSourceOn()
        {
            AcSource acSource = AcSource.GetObj();

            // 전원 ON
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = acSource.AcPowerCtrl(CtrlFlag.ON);

                if (result == CtrlFlag.ON)
                    return StateFlag.PASS;
            }
            return StateFlag.AC_ON_ERR;
        }

        /**
         *  @brief AC 출력 OFF
         *  @details AC 소스의 출력을 OFF한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag AcSourceOff()
        {
            AcSource acSource = AcSource.GetObj();

            // 전원 OFF
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = acSource.AcPowerCtrl(CtrlFlag.OFF);

                if (result == CtrlFlag.OFF)
                    return StateFlag.PASS;
            }
            return StateFlag.AC_OFF_ERR;
        }

        /**
         *  @brief DC 설정
         *  @details DC 소스의 전압, 전류를 설정한다
         *  
         *  @param double dcv - DC 전압
         *  @param double dcc - DC 전류
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag DcSourceSet(double dcv, double dcc)
        {
            DcSource dcSource = DcSource.GetObj();
            double? result = null;

            // 전압 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                result = dcSource.DcVoltSet(dcv);
                if (result == dcv) break;
            }
            if (result != dcv) return StateFlag.DC_VOLT_SET_ERR;

            // 전류 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                result = dcSource.DcCurrSet(dcc);
                if (result == dcc) break;
            }
            if (result != dcc) return StateFlag.DC_CURR_SET_ERR;

            return StateFlag.PASS;
        }

        /**
         *  @brief DC 출력 ON
         *  @details DC 소스의 출력을 ON한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag DcSourceOn()
        {
            DcSource dcSource = DcSource.GetObj();

            // 전원 ON
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = dcSource.DcPowerCtrl(CtrlFlag.ON);

                if (result == CtrlFlag.ON)
                    return StateFlag.PASS;
            }
            return StateFlag.DC_ON_ERR;
        }

        /**
         *  @brief DC 출력 OFF
         *  @details DC 소스의 출력을 OFF한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag DcSourceOff()
        {
            DcSource dcSource = DcSource.GetObj();

            // 전원 OFF
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = dcSource.DcPowerCtrl(CtrlFlag.OFF);

                if (result == CtrlFlag.OFF)
                    return StateFlag.PASS;
            }
            return StateFlag.DC_OFF_ERR;
        }

        /**
         *  @brief 부하 설정
         *  @details 부하의 전류를 설정한다
         *  
         *  @param double curr - 부하 전류
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag LoadCurrSet(double curr)
        {
            DcLoad dcLoad = DcLoad.GetObj();

            dcLoad.Remote();
            // 부하 설정
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                double? result = dcLoad.CurrSet(curr);
                if (result == curr)
                    return StateFlag.PASS;
            }
            return StateFlag.LOAD_SET_ERR;
        }

        /**
         *  @brief 부하 출력 ON
         *  @details 부하의 출력을 ON한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag LoadOn()
        {
            DcLoad dcLoad = DcLoad.GetObj();

            dcLoad.Remote();
            // 전원 ON
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = dcLoad.LoadPowerCtrl(CtrlFlag.ON);
                if (result == CtrlFlag.ON)
                    return StateFlag.PASS;
            }
            return StateFlag.LOAD_ON_ERR;
        }

        /**
         *  @brief 부하 출력 OFF
         *  @details 부하의 출력을 OFF한다
         *  
         *  @param
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag LoadOff()
        {
            DcLoad dcLoad = DcLoad.GetObj();

            dcLoad.Remote();
            // 전원 OFF
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                CtrlFlag? result = dcLoad.LoadPowerCtrl(CtrlFlag.OFF);
                if (result == CtrlFlag.OFF)
                    return StateFlag.PASS;
            }
            return StateFlag.LOAD_OFF_ERR;
        }

        /**
         *  @brief AC 소스 수동조작 윈도우
         *  @details 서브 윈도우를 띄워 AC 소스 수동조작을 유도한다
         *  
         *  @param double acSetVolt - 설정해야 할 AC 전압
         *  @param double errRange - 허용오차
         *  @param AcCheckMode acMode - AC 상태 감지 모드(노멀, 저전압, 고전압, 정전)
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag AcCtrlWin(double acSetVolt, int errRange, AcCheckMode acMode)
        {
            StateFlag result;

            AcCtrlViewModel.SetAcCheck(acSetVolt, errRange, acMode);
            AcCtrlWindow acCtrlWindow = new AcCtrlWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            acCtrlWindow.ShowDialog();
            subWinResult = AcCtrlViewModel.CtrlResult;

            // AC파워 조작 상태에 따른 분기
            if (subWinResult == true)
                result = StateFlag.PASS;
            else
                result = StateFlag.TEST_PAUSE;

            return result;
        }

        /**
         *  @brief 부하 수동조작 윈도우
         *  @details 서브 윈도우를 띄워 부하 수동조작을 유도한다
         *  
         *  @param double loadSetVolt - 설정해야 할 부하값
         *  @param double errRange - 허용오차
         *  @param AcCheckMode acMode - AC 상태 감지 모드(노멀, 과부하)
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag LoadCtrlWin(double loadSetVolt, int errRange, LoadCheckMode loadMode)
        {
            StateFlag result;

            LoadCtrlViewModel.SetLoadCheck(loadSetVolt, errRange, loadMode);
            LoadCtrlWindow loadCtrlWindow = new LoadCtrlWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            loadCtrlWindow.ShowDialog();
            subWinResult = LoadCtrlViewModel.CtrlResult;

            // 부하 조작 상태에 따른 분기
            if (subWinResult == true)
                result = StateFlag.PASS;
            else
                result = StateFlag.TEST_PAUSE;

            return result;
        }

        /**
         *  @brief 부하 수동조작 윈도우(출하용)
         *  @details 서브 윈도우를 띄워 부하 수동조작을 유도한다(출하용)
         *  
         *  @param double loadSetVolt - 설정해야 할 부하값
         *  @param double errRange - 허용오차
         *  @param AcCheckMode acMode - AC 상태 감지 모드(노멀, 과부하)
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag LoadCtrlWin2(double loadSetVolt, int errRange, LoadCheckMode loadMode)
        {
            StateFlag result;

            LoadCtrlViewModel.SetLoadCheck(loadSetVolt, errRange, loadMode);
            LoadCtrlWindow2 loadCtrlWindow = new LoadCtrlWindow2
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            loadCtrlWindow.ShowDialog();
            subWinResult = LoadCtrlViewModel.CtrlResult;

            // 부하 조작 상태에 따른 분기
            if (subWinResult == true)
                result = StateFlag.PASS;
            else
                result = StateFlag.TEST_PAUSE;

            return result;
        }

        /**
         *  @brief 성적서 작성
         *  @details 테스트 후 산출된 데이터를 파일에 저장한다
         *  
         *  @param int Order - 테스트 번호
         *  @param (string, string)? resultData - 테스트 결과값(값, 결과)
         *  
         *  @return StateFlag - 설정 결과
         */
        protected StateFlag ResultDataSave(int Order, (string, string)? resultData)
        {
            CsvReport csvReport = CsvReport.GetObj();

            List<string[]> testList = csvReport.CsvReader(ReportSavePath);

            testList[Order][2] = resultData.Value.Item1;    // 테스트 값
            testList[Order][3] = resultData.Value.Item2;    // 테스트 결과

            StateFlag saveState = csvReport.ReportSave(ReportSavePath, testList);

            return saveState;
        }

        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
        public abstract StateFlag TestSeq(int stepNumber, ref int jumpStepNum);
        /**
         *  @brief 테스트 UI 텍스트 색 변경
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 변경시킨다
         *  
         *  @param int index - 세부 단계의 인덱스
         *  @param StateFlag stateFlag - 세부 단계의 테스트 결과
         *  
         *  @return
         */
        public abstract void TextColorChange(int index, StateFlag stateFlag);
        /**
         *  @brief 테스트 UI 텍스트 색 리셋
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 흰색으로 리셋
         *  
         *  @param 
         *  
         *  @return
         */
        public abstract void UiReset();
    }
}
