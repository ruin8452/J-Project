﻿using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief 출력 전압 체크 테스트 클래스
     *  @details 출력 전압 체크와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class DcOutCheckVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            SWITCH_CHECK,
            OUTPUT_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private enum RefSave
        {
            NO_SAVE,
            SAVE
        }

        private const double ERR_RATE = 0.05;

        private int TestOrterNum = (int)SecondTestOrder.OutputCheck;
        public static string TestName { get; } = "출력전압 체크";
        public DcOutCheck DcCheck { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        public DcOutCheckVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            DcOutCheck.Load();
            DcCheck = DcOutCheck.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            SecondOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName, "판단불가", "불합격" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<object>(UnitTestClick);
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
            DcOutCheck.Save();
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
                case Seq.AC_ON: // 초기 AC 설정
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(DcCheck.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(DcCheck.AcVolt[caseNumber], DcCheck.AcCurr[caseNumber], DcCheck.AcFreq[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(DcCheck.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(DcCheck.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.SWITCH_CHECK:
                    TestLog.AppendLine("[ 스위치 체크 ]");
                    Rectifier rect = Rectifier.GetObj();
                    MessageBoxResult msgResult;

                    if (rect.LocalRemoteLed && rect.SwLed[0] && rect.SwLed[1] && rect.SwLed[2] && rect.SwLed[3])
                    {
                        TestLog.AppendLine($"- 스위치 체크 완료\n");
                        result = StateFlag.PASS;
                    }
                    else
                    {
                        msgResult = MessageBox.Show("스위치를 Local 및 ON으로 변경해주세요.", "스위치 변경", MessageBoxButton.OKCancel);
                        if (msgResult == MessageBoxResult.Cancel)
                        {
                            TestLog.AppendLine($"- 스위치 체크 실패\n");
                            resultData = ("스위치 조작 오류", "불합격");
                            result = StateFlag.LOCAL_SWITCH_ERR;
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                        }
                        else
                            result = StateFlag.WAIT;
                    }
                    break;

                case Seq.OUTPUT_CHECK:
                    TestLog.AppendLine("[ 출력 체크 ]");

                    TestLog.AppendLine("[ 기본값 세팅 ]");
                    result = DefaultRefSet(DcCheck.DefaultRef[caseNumber]);
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    TestLog.AppendLine($"- 정상 출력값 : 53.3");
                    TestLog.AppendLine($"- 정류기 출력전압 : {Rectifier.GetObj().DcOutputVolt}");
                    TestLog.AppendLine($"- 현재오차 : {Math.Abs(53.3 - Rectifier.GetObj().DcOutputVolt)}");
                    TestLog.AppendLine($"- 허용오차 : 1");

                    if (Math.Abs(53.3 - Rectifier.GetObj().DcOutputVolt) <= 1)
                    {
                        TestLog.AppendLine($"- 테스트 합격\n");
                        resultData = ((-Rectifier.GetObj().DcOutputVolt).ToString(), "합격");
                        result = StateFlag.PASS;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 테스트 불합격\n");
                        resultData = ((-Rectifier.GetObj().DcOutputVolt).ToString(), "불합격");
                        result = StateFlag.NORMAL_ERR;
                    }
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(DcCheck.NextTestWait[caseNumber]);
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

        /**
         *  @brief DC 전압 기본값 CAL 수행
         *  @details DC 전압 기본값 CAL을 진행하고 잘 됐는지 검사한다
         *  
         *  @param double refValue - 포인트 CAL 기준값
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag DefaultRefSet(double refValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            double rectDcVolt;
            bool cmdResult;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                TestLog.Append($"- 전압 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)(refValue * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1);

                // CAL 완료 후 출력값 보정
                dmmDcVolt = Dmm1.GetObj().DcVolt;
                if (dmmDcVolt < refValue)
                {
                    TestLog.AppendLine($"- CAL 보정 : 설정값 보다 낮음");

                    double correction = refValue - dmmDcVolt;
                    TestLog.Append($"- 전압 Ref 보정 설정({correction}) -> ");
                    cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)((refValue + correction) * 100));
                    if (!cmdResult)
                    {
                        TestLog.AppendLine($"실패");
                        continue;
                    }
                    TestLog.AppendLine($"성공");
                }
                else if (dmmDcVolt > refValue + 0.03)
                {
                    TestLog.AppendLine($"- CAL 보정 : 설정값 보다 높음");

                    double correction = dmmDcVolt - refValue;
                    TestLog.Append($"- 전압 Ref 보정 설정({correction}) -> ");
                    cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)((refValue - correction) * 100));
                    if (!cmdResult)
                    {
                        TestLog.AppendLine($"실패");
                        continue;
                    }
                    TestLog.AppendLine($"성공");
                }

                // 최종 출력값 검사
                Util.Delay(1);
                rectDcVolt = rect.DcOutputVolt;

                if (Math.Abs(refValue - rectDcVolt) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- 설정 성공");
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- 설정 실패");
            return StateFlag.DC_VOLT_CAL_ERR;
        }
    }
}