﻿using J_Project.Communication.CommFlags;
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
        IdSet,         // ID 세팅
        LocalSwitch,   // 로컬 스위치 확인
        Remote,        // 리모트 통신 확인
        Battery,       // 배터리 통신 확인
        Temp,          // 온도센서 점검
        NoLoad,        // 무부하
        LoadReg0,      // 로드 레귤레이션0
        LoadReg1,      // 로드 레귤레이션1
        LoadReg2,      // 로드 레귤레이션2
        LineReg0,      // 라인 레귤레이션0
        LineReg1,      // 라인 레귤레이션1
        LineReg2,      // 라인 레귤레이션2
        Noise,         // 리플 노이즈
        PowerFacter,   // 역률
        Efficiency,    // 효율
        OutputLow,     // 출력 저전압
        OutputHigh,    // 출력 고전압
        AcLow0,        // AC 저전압0
        AcLow1,        // AC 저전압1
        AcHigh,        // AC 고전압
        AcBlackOut,    // AC 정전
        OverLoad0,     // 출력 과부하
        OverLoad1,     // 출력 과부하
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

        public int TotalStepNum;
        public static string ReportSavePath;

        // 첫 줄 기본 정보 삽입을 위해 배열의 크기 1+
        private static int FirstOrderCnt = Enum.GetNames(typeof(FirstTestOrder)).Length + 1;
        private static int SecondOrderCnt = Enum.GetNames(typeof(SecondTestOrder)).Length + 1;
        public static string[][] FirstOrder = new string[FirstOrderCnt][];
        public static string[][] SecondOrder = new string[SecondOrderCnt][];

        protected (string, string) resultData = ("판단 불가", "불합격");
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
                FirstOrder[i][3] = "불합격";
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
            for (int i = 0; i < SecondOrderCnt; i++)
            {
                SecondOrder[i][2] = "판단 불가";
                SecondOrder[i][3] = "불합격";
            }
        }

        // AC 설정
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
        // AC 설정
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

        // AC On
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

        // AC Off
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

        // DC 설정
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

        // DC On
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

        // DC Off
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

        // 부하 설정
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

        // Load On
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

        // Load Off
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

        // 성적서 작성
        protected StateFlag ResultDataSave(int Order, string testName, (string, string)? resultData)
        {
            CsvReport csvReport = CsvReport.GetObj();

            List<string[]> testList = csvReport.CsvReader(ReportSavePath);

            testList[Order][2] = resultData.Value.Item1;    // 테스트 값
            testList[Order][3] = resultData.Value.Item2;    // 테스트 결과

            StateFlag saveState = csvReport.ReportSave(ReportSavePath, testList);

            return saveState;
        }

        public abstract StateFlag TestSeq(int caseNumber, int stepNumber, ref int jumpStepNum);
        public abstract void TextColorChange(int index, StateFlag stateFlag);
        public abstract void UiReset();
    }
}
