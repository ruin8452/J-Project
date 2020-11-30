using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.UI.SubWindow;
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
     *  @brief 리플 노이즈 테스트 클래스(출하용)
     *  @details 리플 노이즈와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class Noise2VM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            LOAD_ON,
            DELAY2,
            RESULT_CHECK,
            RESULT_SAVE,
            LOAD_OFF,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private int TestOrterNum = (int)SecondTestOrder.Noise;
        public static string TestName { get; } = "리플 노이즈";
        public 리플_노이즈 Noise { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public Noise2VM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            리플_노이즈.Load();
            Noise = 리플_노이즈.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            SecondOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName, "판단불가", "불합격" };

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
            리플_노이즈.Save();
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
                    if (Math.Abs(Noise.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(Noise.AcVolt[caseNumber], Noise.AcCurr[caseNumber], Noise.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(Noise.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(Noise.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    // 팝업 전 확인 후 통과
                    double LoadCurr = Rectifier.GetObj().DcOutputCurr;
                    TestLog.AppendLine($"- 부하 : {LoadCurr}");
                    if (Math.Abs(Noise.LoadCurr[caseNumber] - LoadCurr) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(Noise.LoadCurr[caseNumber]);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin2(Noise.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.SECOND_TEST);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(Noise.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단 & 성적서 작성
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = NoiseCheckTest(Noise.LimitNoiseVolt[caseNumber], ref resultData);

                    // 실패시 수동 입력
                    if (result != StateFlag.PASS)
                        result = NoisePassiveCheckTest(Noise.LimitNoiseVolt[caseNumber], ref resultData);

                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.LOAD_OFF: // Load OFF
                    TestLog.AppendLine("[ 부하 OFF ]");

                    if (!TestOption.GetObj().IsLoadManage)
                    {
                        TestLog.AppendLine("- 부하 관리 OFF");
                        result = StateFlag.PASS;
                        break;
                    }

                    // 팝업 전 확인 후 통과
                    LoadCurr = Rectifier.GetObj().DcOutputCurr;
                    TestLog.AppendLine($"- 부하 : {LoadCurr}");
                    if (Math.Abs(0 - LoadCurr) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadOff();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin2(0, LOAD_ERR_RANGE, LoadCheckMode.SECOND_TEST);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(Noise.NextTestWait[caseNumber]);
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
         *  @brief 노이즈 검사
         *  @details 출력 시 발생하는 노이즈가 정상 범위 이내인지 검사
         *  
         *  @param int limitNoiseVolt - 기준 노이즈값
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag NoiseCheckTest(int limitNoiseVolt, ref (string, string) resultData)
        {
            Oscilloscope osc = Oscilloscope.GetObj();
            double noiseVolt = osc.RealPk2pk();

            noiseVolt = Math.Round(noiseVolt, 3) * 1000;
            TestLog.AppendLine($"- 노이즈 : {noiseVolt}");

            if (noiseVolt <= limitNoiseVolt)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (noiseVolt.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 초과");
                resultData = (noiseVolt.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }

        /**
         *  @brief 노이즈 검사(수동입력)
         *  @details 출력 시 발생하는 노이즈가 정상 범위 이내인지 검사
         *           계측장비와의 통신이 원할하지 않을 경우 수동입력을 통해 진행한다
         *  
         *  @param int limitNoiseVolt - 기준 노이즈값
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag NoisePassiveCheckTest(int limitNoiseVolt, ref (string, string) resultData)
        {
            TestLog.AppendLine($"- 노이즈 팝업");

            TextWindow textWindow = new TextWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            textWindow.ShowDialog();
            double noiseVolt = TextViewModel.Number;

            TestLog.AppendLine($"- 노이즈 : {noiseVolt}");

            if (noiseVolt <= limitNoiseVolt)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (noiseVolt.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 초과");
                resultData = (noiseVolt.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}