﻿using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief 출력 과부하 보호 테스트 클래스
     *  @details 출력 과부하 보호와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class OutputOverVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            OVER_LOAD,
            DELAY2,
            LOAD_OFF,
            DELAY3,
            RESULT_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "출력 과부하 보호";
        public 출력_과부하_보호 OutputOver { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public OutputOverVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            출력_과부하_보호.Load();
            OutputOver = 출력_과부하_보호.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataSave()
        {
            출력_과부하_보호.Save();
        }

        /**
         *  @brief 테스트 UI 텍스트 색 변경
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 변경시킨다
         *  
         *  @param int index - 세부 단계의 인덱스
         *  @param StateFlag stateFlag - 세부 단계의 테스트 결과
         *  
         *  @return
         */
        public override void TextColorChange(int index, StateFlag stateFlag)
        {
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.TEST_END)
                ButtonColor[index] = Brushes.GreenYellow;
            else if (stateFlag == StateFlag.WAIT)
                ButtonColor[index] = Brushes.Yellow;
            else
                ButtonColor[index] = Brushes.Red;
        }

        /**
         *  @brief 테스트 UI 텍스트 색 리셋
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 흰색으로 리셋
         *  
         *  @param 
         *  
         *  @return
         */
        public override void UiReset()
        {
            for (int i = 0; i < ButtonColor.Count; i++)
                ButtonColor[i] = Brushes.White;
        }

        /**
         *  @brief 수동 테스트 동작
         *  @details 수동 모드 운영 시, 테스트 UI의 활성화된 버튼을 클릭했을 경우 실행
         *  
         *  @param object value - 2개의 데이터로 구성(1. 해당 테스트의 케이스 번호, 2. 해당 세부 단계의 인덱스 번호)
         *  
         *  @return
         */
        private void UnitTestClick(object value)
        {
            object[] parameter = (object[])value;

            //string result = Test.EquiConnectCheck();
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            int caseIndex = int.Parse(parameter[0].ToString());
            int unitIndex = int.Parse(parameter[1].ToString());
            int jumpNum = -1;

            TextColorChange(unitIndex, StateFlag.WAIT);
            StateFlag resultState = TestSeq(caseIndex, unitIndex, ref jumpNum);
            TextColorChange(unitIndex, resultState);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // 시퀀스 관련
        ///////////////////////////////////////////////////////////////////////////////////
        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int caseNumbere - 해당 테스트의 케이스 번호
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
        public override StateFlag TestSeq(int caseNumber, int stepNumber, ref int jumpStepNum)
        {
            StateFlag result = StateFlag.NORMAL_ERR;
            Seq stepName = (Seq)stepNumber;

            switch (stepName)
            {
                case Seq.AC_ON:
                    TestLog.AppendLine("[ AC ]");

                    if (Option.IsFullAuto)
                    {
                        double acVolt = PowerMeter.GetObj().AcVolt;
                        TestLog.AppendLine($"- AC : {acVolt}");
                        if (Math.Abs(OutputOver.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                        {
                            result = StateFlag.PASS;
                            break;
                        }

                        result = AcSourceSet(OutputOver.AcVolt[caseNumber], OutputOver.AcCurr[caseNumber], OutputOver.AcFreq[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(OutputOver.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(OutputOver.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.OVER_LOAD:
                    TestLog.AppendLine("[ 과부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(OutputOver.OverLoadCurr[caseNumber] - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(OutputOver.OverLoadCurr[caseNumber]);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = OverLoadCheck(caseNumber);
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(OutputOver.OverLoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.OVER_LOAD);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(OutputOver.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_OFF: // Load OFF
                    TestLog.AppendLine("[ 부하 OFF ]");

                    if (Option.IsFullAuto)
                    {
                        result = LoadOff();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(0, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(OutputOver.Delay3[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = PassFailCheckTest(caseNumber, OutputOver.CheckTiming[caseNumber], OutputOver.DcOutVolt[caseNumber], OutputOver.VoltErrRate[caseNumber], ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    if (caseNumber == 0)
                        result = ResultDataSave((int)FirstTestOrder.OverLoad0, TestName, resultData);
                    else
                        result = ResultDataSave((int)FirstTestOrder.OverLoad1, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(OutputOver.NextTestWait[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.END_TEST:
                    TestLog.AppendLine("[ 테스트 완료 ]\n");
                    result = StateFlag.TEST_END;
                    break;

                default:
                    break;
            }
            return result;
        }

        // 과부하 보호 검사
        private StateFlag OverLoadCheck(int caseNum)
        {
            TestLog.Append($"- Fault 발생 검사 -> ");

            // 5초 동안 검사
            for (int i = 0; i < 5; i++)
            {
                Util.Delay(1);

                if (Rectifier.GetObj().Flag_DcOverLoad)
                {
                    TestLog.AppendLine($"- Fault 발생");
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- Fault 미발생");
            return StateFlag.FAULT_ERROR;
        }

        private StateFlag PassFailCheckTest(int caseNum, double timing, double dcOutVolt, double errRate, ref (string, string) resultData)
        {
            Dmm1 dmm = Dmm1.GetObj();
            double voltCheck = double.NaN;

            for (int i = 0; i < timing; i++)
            {
                Util.Delay(1);  // 설정 제한시간 후 체크
                voltCheck = Math.Round(dmm.DcVolt, 3);

                TestLog.AppendLine($"- {i + 1}초 : {voltCheck}");

                if (dcOutVolt - errRate < voltCheck && voltCheck < dcOutVolt + errRate)
                    break;
            }

            TestLog.AppendLine($"- 기준값 : {dcOutVolt}");
            TestLog.AppendLine($"- 측정값 : {voltCheck}");
            TestLog.AppendLine($"- 현재오차 : {Math.Abs(dcOutVolt - voltCheck)}");
            TestLog.AppendLine($"- 허용오차 : {errRate}");

            if (Math.Abs(dcOutVolt - voltCheck) <= errRate)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = ((-voltCheck).ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한 시간 내에 출력 전압 복귀 실패");
                resultData = ((-voltCheck).ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}